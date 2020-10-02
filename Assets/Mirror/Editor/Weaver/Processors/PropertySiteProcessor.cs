using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirror.Weaver
{
    public static class PropertySiteProcessor
    {
        public static void Process(ModuleDefinition moduleDef)
        {
            DateTime startTime = DateTime.Now;

            //Search through the types
            foreach (TypeDefinition td in moduleDef.Types)
            {
                if (td.IsClass)
                {
                    ProcessSiteClass(td);
                }
            }

            if (Weaver.WeaveLists.generateContainerClass != null)
            {
                moduleDef.Types.Add(Weaver.WeaveLists.generateContainerClass);
            }

            Console.WriteLine("  ProcessSitesModule " + moduleDef.Name + " elapsed time:" + (DateTime.Now - startTime));
        }

        static void ProcessSiteClass(TypeDefinition td)
        {
            foreach (MethodDefinition md in td.Methods)
            {
                ProcessSiteMethod(md);
            }

            foreach (TypeDefinition nested in td.NestedTypes)
            {
                ProcessSiteClass(nested);
            }
        }

        static void ProcessSiteMethod(MethodDefinition md)
        {
            // process all references to replaced members with properties

            if (md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName ||
                md.Name.StartsWith(Weaver.InvokeRpcPrefix))
                return;

            if (md.IsAbstract)
            {
                return;
            }

            if (md.Body != null && md.Body.Instructions != null)
            {
                Instruction instr = md.Body.Instructions[0];

                while (instr != null)
                {
                    instr = ProcessInstruction(md, instr);
                    instr = instr.Next;
                }
            }
        }

        // replaces syncvar write access with the NetworkXYZ.get property calls
        static void ProcessInstructionSetterField(MethodDefinition md, Instruction i, FieldDefinition opField)
        {
            // dont replace property call sites in constructors
            if (md.Name == ".ctor")
                return;

            // does it set a field that we replaced?
            if (Weaver.WeaveLists.replacementSetterProperties.TryGetValue(opField, out MethodDefinition replacement))
            {
                //replace with property
                i.OpCode = OpCodes.Call;
                i.Operand = replacement;
            }
        }

        // replaces syncvar read access with the NetworkXYZ.get property calls
        static void ProcessInstructionGetterField(MethodDefinition md, Instruction i, FieldDefinition opField)
        {
            // dont replace property call sites in constructors
            if (md.Name == ".ctor")
                return;

            // does it set a field that we replaced?
            if (Weaver.WeaveLists.replacementGetterProperties.TryGetValue(opField, out MethodDefinition replacement))
            {
                //replace with property
                i.OpCode = OpCodes.Call;
                i.Operand = replacement;
            }
        }

        static Instruction ProcessInstruction(MethodDefinition md, Instruction instr)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldDefinition opFieldst)
            {
                // this instruction sets the value of a field. cache the field reference.
                ProcessInstructionSetterField(md, instr, opFieldst);
            }

            if (instr.OpCode == OpCodes.Ldfld && instr.Operand is FieldDefinition opFieldld)
            {
                // this instruction gets the value of a field. cache the field reference.
                ProcessInstructionGetterField(md, instr, opFieldld);
            }

            if (instr.OpCode == OpCodes.Ldflda && instr.Operand is FieldDefinition opFieldlda)
            {
                // loading a field by reference,  watch out for initobj instruction
                // see https://github.com/vis2k/Mirror/issues/696
                return ProcessInstructionLoadAddress(md, instr, opFieldlda);
            }

            return instr;
        }

        static Instruction ProcessInstructionLoadAddress(MethodDefinition md, Instruction instr, FieldDefinition opField)
        {
            // dont replace property call sites in constructors
            if (md.Name == ".ctor")
                return instr;

            // does it set a field that we replaced?
            if (Weaver.WeaveLists.replacementSetterProperties.TryGetValue(opField, out MethodDefinition replacement))
            {
                // we have a replacement for this property
                // is the next instruction a initobj?
                Instruction nextInstr = instr.Next;

                if (nextInstr.OpCode == OpCodes.Initobj)
                {
                    // we need to replace this code with:
                    //     var tmp = new MyStruct();
                    //     this.set_Networkxxxx(tmp);
                    ILProcessor worker = md.Body.GetILProcessor();
                    var tmpVariable = new VariableDefinition(opField.FieldType);
                    md.Body.Variables.Add(tmpVariable);

                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloca, tmpVariable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Initobj, opField.FieldType));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloc, tmpVariable));
                    Instruction newInstr = worker.Create(OpCodes.Call, replacement);
                    worker.InsertBefore(instr, newInstr);

                    worker.Remove(instr);
                    worker.Remove(nextInstr);

                    return newInstr;
                }
            }

            return instr;
        }
    }
}
