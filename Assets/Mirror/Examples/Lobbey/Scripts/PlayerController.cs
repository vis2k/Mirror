using UnityEngine;

namespace Mirror.Examples.NetworkLobby
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [SyncVar]
        public int Index;

        [SyncVar]
        public uint score = 0;

        [SyncVar(hook = nameof(SetColor))]
        public Color playerColor = Color.black;

        CharacterController characterController;

        public float moveSpeed = 300f;

        public float horiz = 0f;
        public float vert = 0f;
        public float turn = 0f;

        public float turnSpeedAccel = 30;
        public float turnSpeedDecel = 30;
        public float maxTurnSpeed = 100;

        Vector3 direction = Vector3.zero;
        GameObject controllerColliderHitObject;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            characterController = GetComponent<CharacterController>();

            // Turn off main camera because GamePlayer prefab has its own camera
            GetComponentInChildren<Camera>().enabled = true;
            Camera.main.enabled = false;
        }

        void SetColor(Color color)
        {
            //Debug.LogWarningFormat("PlayerController SetColor netId:{0} to {1}", netId, color);
            GetComponent<Renderer>().material.color = color;
        }

        void Start()
        {
            // This is a workaround pending a fix for https://github.com/vis2k/Mirror/issues/372
            SetColor(playerColor);
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            horiz = Input.GetAxis("Horizontal");
            vert = Input.GetAxis("Vertical");

            if ((Input.GetKey(KeyCode.Q)) && (turn > -maxTurnSpeed))
                turn = turn - turnSpeedAccel;
            else if ((Input.GetKey(KeyCode.E)) && (turn < maxTurnSpeed))
                turn = turn + turnSpeedAccel;
            else
            {
                if (turn > turnSpeedDecel)
                    turn = turn - turnSpeedDecel;
                else if (turn < -turnSpeedDecel)
                    turn = turn + turnSpeedDecel;
                else
                    turn = 0;
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null) return;

            transform.Rotate(0f, turn * Time.deltaTime, 0f);

            direction = transform.TransformDirection((Vector3.ClampMagnitude(new Vector3(horiz, 0, vert), 1) * moveSpeed));
            characterController.SimpleMove(direction * Time.fixedDeltaTime);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If player and prize objects are on their own layer(s) with correct
            // collision matrix, we wouldn't have to validate the hit.gameobject.
            // Since this is just an example, project settings aren't included so we check the name.

            controllerColliderHitObject = hit.gameObject;

            if (isLocalPlayer && controllerColliderHitObject.name == "Prize")
            {
                if (LogFilter.Debug) Debug.LogFormat("OnControllerColliderHit {0}[{1}] with {2}[{3}]", name, netId, controllerColliderHitObject.name, controllerColliderHitObject.GetComponent<NetworkIdentity>().netId);

                // Disable the prize gameobject so it doesn't impede player movement
                // It's going to be destroyed in a few frames and we don't want to spam CmdClaimPrize.
                // OnControllerColliderHit will fire many times as the player slides against the object.
                controllerColliderHitObject.SetActive(false);

                CmdClaimPrize(controllerColliderHitObject);
            }
        }

        [Command]
        public void CmdClaimPrize(GameObject hitObject)
        {
            // Null check is required, otherwise close timing of multiple claims could throw a null ref.
            if (hitObject != null)
                hitObject.GetComponent<Reward>().ClaimPrize(gameObject);
        }

        void OnGUI()
        {
            GUI.Box(new Rect(10 + (Index * 110), 10, 100, 25), score.ToString().PadLeft(10));
        }
    }
}
