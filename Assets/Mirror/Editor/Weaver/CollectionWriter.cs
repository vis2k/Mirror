using Mono.CecilX;
using Mono.CecilX.Cil;

namespace Mirror.Weaver
{
    public class ArrayWriter : CollectionWriter
    {
        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        public ArrayWriter(TypeReference variable, int recursionCount) : base(variable, recursionCount) { }

        protected override string namePrefix => "Array";
        protected override bool needNullCheck => true;

        protected override TypeReference GetElementType(TypeReference variable)
        {
            return variable.GetElementType();
        }
        protected override void ValidateType(TypeReference variable)
        {
            if (!variable.IsArrayType())
            {
                throw new GenerateWriterException($"{variable.Name} is an unsupported type. Jagged and multidimensional arrays are not supported", variable);
            }
        }
        protected override Instruction CreateLengthInstruction(ILProcessor worker)
        {
            return worker.Create(OpCodes.Ldlen);
        }
    }
    public class ArraySegmentWriter : CollectionWriter
    {
        GenericInstanceType genericInstance;

        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        public ArraySegmentWriter(TypeReference variable, int recursionCount) : base(variable, recursionCount) { }

        protected override string namePrefix => "ArraySegment";
        protected override bool needNullCheck => false;

        protected override TypeReference GetElementType(TypeReference variable)
        {
            genericInstance = (GenericInstanceType)variable;
            TypeReference elementType = genericInstance.GenericArguments[0];
            return elementType;
        }

        protected override Instruction CreateLengthInstruction(ILProcessor worker)
        {
            MethodReference countref = WeaverTypes.ArraySegmentCountReference.MakeHostInstanceGeneric(genericInstance);

            return worker.Create(OpCodes.Call, countref);
        }

        protected override void AppendCollection(ILProcessor worker)
        {
            MethodReference getArray = WeaverTypes.ArraySegmentArrayReference.MakeHostInstanceGeneric(genericInstance);

            // arg1 is the segment
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            // value.Array
            worker.Append(worker.Create(OpCodes.Call, getArray));
        }

        protected override void AppendIndex(ILProcessor worker)
        {
            MethodReference getOffset = WeaverTypes.ArraySegmentOffsetReference.MakeHostInstanceGeneric(genericInstance);

            // local 1 is i
            worker.Append(worker.Create(OpCodes.Ldloc_1));

            // value.Offset
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, getOffset));

            // add (i + offset)
            worker.Append(worker.Create(OpCodes.Add));
        }
    }
    public class ListWriter : CollectionWriter
    {
        GenericInstanceType genericInstance;

        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        public ListWriter(TypeReference variable, int recursionCount) : base(variable, recursionCount) { }

        protected override string namePrefix => "List";
        protected override bool needNullCheck => true;

        protected override TypeReference GetElementType(TypeReference variable)
        {
            genericInstance = (GenericInstanceType)variable;
            TypeReference elementType = genericInstance.GenericArguments[0];
            return elementType;
        }

        protected override Instruction CreateLengthInstruction(ILProcessor worker)
        {
            MethodReference countref = WeaverTypes.ListCountReference.MakeHostInstanceGeneric(genericInstance);

            return worker.Create(OpCodes.Call, countref);
        }

        protected override void AppendGetElement(ILProcessor worker)
        {
            MethodReference getItem = WeaverTypes.ListGetItemReference.MakeHostInstanceGeneric(genericInstance);

            // call get_Item
            // list and index should be already on stack
            worker.Append(worker.Create(OpCodes.Call, getItem));
        }

    }
    public abstract class CollectionWriter
    {
        private readonly TypeReference variable;

        private readonly TypeReference elementType;
        private readonly MethodReference elementWriteFunc;
        private readonly MethodReference intWriterFunc;


        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        public CollectionWriter(TypeReference variable, int recursionCount)
        {
            this.variable = variable;

            ValidateType(variable);

            elementType = GetElementType(variable);
            elementWriteFunc = Writers.GetWriteFunc(elementType, recursionCount + 1);
            intWriterFunc = Writers.GetWriteFunc(WeaverTypes.Import<int>());

            // in later PR throw here if elementWriteFunc is null, see null check top of Create
        }

        protected abstract string namePrefix { get; }
        protected abstract bool needNullCheck { get; }

        protected virtual void ValidateType(TypeReference variable) { /* no validation */ }
        protected abstract TypeReference GetElementType(TypeReference variable);

        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        public MethodDefinition Create()
        {
            // need this null check till later PR when GetWriteFunc throws exception instead
            if (elementWriteFunc == null)
            {
                Weaver.Error($"Cannot generate writer for Array because element {elementType.Name} does not have a writer. Use a supported type or provide a custom writer", variable);
                return null;
            }


            string functionName = createFunctionName(elementType);
            MethodDefinition writerFunc = createMethod(functionName);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            if (needNullCheck)
            {
                AppendNullCheck(worker);
            }

            AppendWriteLength(worker);

            // labels that can be jumped to
            Instruction labelCheckLength = worker.Create(OpCodes.Nop);
            Instruction labelLoopBody = worker.Create(OpCodes.Nop);

            AppendStartLoop(worker, labelCheckLength, labelLoopBody);

            AppendWriteElement(worker);

            AppendEndLoop(worker, labelCheckLength, labelLoopBody);

            AppendReturn(worker);

            return writerFunc;
        }

        string createFunctionName(TypeReference elementType)
        {
            string functionName = $"_Write{namePrefix}_{elementType.Name}_";
            if (variable.DeclaringType != null)
            {
                functionName += variable.DeclaringType.Name;
            }
            else
            {
                functionName += "None";
            }

            return functionName;
        }
        MethodDefinition createMethod(string functionName)
        {
            // create new writer for this type
            MethodDefinition writerFunc = new MethodDefinition(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig,
                    WeaverTypes.Import(typeof(void)));

            writerFunc.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, WeaverTypes.Import<NetworkWriter>()));
            writerFunc.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, Weaver.CurrentAssembly.MainModule.ImportReference(variable)));

            writerFunc.Body.Variables.Add(new VariableDefinition(WeaverTypes.Import<int>()));
            writerFunc.Body.Variables.Add(new VariableDefinition(WeaverTypes.Import<int>()));
            writerFunc.Body.InitLocals = true;
            return writerFunc;
        }
        void AppendNullCheck(ILProcessor worker)
        {
            // if (value == null)
            // {
            //     writer.WritePackedInt32(-1);
            //     return;
            // }
            Instruction labelNull = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Brtrue, labelNull));

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4_M1));
            worker.Append(worker.Create(OpCodes.Call, intWriterFunc));
            worker.Append(worker.Create(OpCodes.Ret));

            // else not null
            worker.Append(labelNull);
        }
        void AppendWriteLength(ILProcessor worker)
        {
            // int length = value.Length;
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(CreateLengthInstruction(worker));
            worker.Append(worker.Create(OpCodes.Stloc_0));

            // writer.WritePackedInt32(length);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldloc_0));
            worker.Append(worker.Create(OpCodes.Call, intWriterFunc));
        }
        protected abstract Instruction CreateLengthInstruction(ILProcessor worker);

        static void AppendStartLoop(ILProcessor worker, Instruction labelCheckLength, Instruction labelLoopBody)
        {
            // for (i = 0; ...
            worker.Append(worker.Create(OpCodes.Ldc_I4_0));
            worker.Append(worker.Create(OpCodes.Stloc_1));

            // jump to length check (appended in AppendEndLoop)
            worker.Append(worker.Create(OpCodes.Br, labelCheckLength));

            worker.Append(labelLoopBody);
        }
        static void AppendEndLoop(ILProcessor worker, Instruction labelCheckLength, Instruction labelLoopBody)
        {
            // ... i++)
            worker.Append(worker.Create(OpCodes.Ldloc_1));
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Add));
            worker.Append(worker.Create(OpCodes.Stloc_1));


            worker.Append(labelCheckLength);
            // ... i < Length; ...
            // i is local 1
            // legnth is local 0
            worker.Append(worker.Create(OpCodes.Ldloc_1));
            worker.Append(worker.Create(OpCodes.Ldloc_0));
            worker.Append(worker.Create(OpCodes.Blt, labelLoopBody));
        }
        static void AppendReturn(ILProcessor worker)
        {
            worker.Append(worker.Create(OpCodes.Ret));
        }


        void AppendWriteElement(ILProcessor worker)
        {
            // writer.Write(value[i]);
            // writer
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // value[i]
            AppendCollection(worker);
            AppendIndex(worker);
            AppendGetElement(worker);

            // Write
            worker.Append(worker.Create(OpCodes.Call, elementWriteFunc));
        }

        protected virtual void AppendCollection(ILProcessor worker)
        {
            // arg1 is the array/list/etc
            worker.Append(worker.Create(OpCodes.Ldarg_1));
        }
        protected virtual void AppendIndex(ILProcessor worker)
        {
            // local 1 is i
            worker.Append(worker.Create(OpCodes.Ldloc_1));
        }
        protected virtual void AppendGetElement(ILProcessor worker)
        {
            // adds  value [i] to stack
            worker.Append(worker.Create(OpCodes.Ldelema, elementType));
            worker.Append(worker.Create(OpCodes.Ldobj, elementType));
        }
    }
}
