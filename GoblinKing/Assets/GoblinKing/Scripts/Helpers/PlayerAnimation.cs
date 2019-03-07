using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    internal class PlayerAnimation : MonoBehaviour
    {
        public Transform LeftHand;
        public Transform RightHand;
        public Core.SmoothMouseLook MouseLook;
        public Transform CameraTransform;
        public Core.Creature PlayerCreature;

        public float ForwardKeyHeldDuration = 0f;

        public bool PeekForward = false;
        public int Peek = 0;

        private float zRotVelocity = 0f;
        private float cameraXVelocity = 0f;
        private float cameraZVelocity = 0f;

        private float attackingTime = 0f;
        private Vector3 leftHandOriginal;
        private Vector3 rightHandOriginal;

        private Quaternion rightHandOriginalRot;
        private Quaternion leftHandOriginalRot;

        private const float WeaponFlyDistance = 0.3f;
        private const float AttackDuration = 0.5f;

        public void StartAttackAnimation()
        {
            attackingTime = AttackDuration;
        }

        private void Awake()
        {
            leftHandOriginal = LeftHand.transform.localPosition;
            rightHandOriginal = RightHand.transform.localPosition;
            leftHandOriginalRot = LeftHand.transform.localRotation;
            rightHandOriginalRot = RightHand.transform.localRotation;
        }

        private float RayDistance(Vector3 localDir, float maxDist)
        {
            Vector3 rayStartPos = Core.Utils.ConvertToWorldCoord(PlayerCreature.Position) + new Vector3(0f, 0.5f, 0f);
            // Vector3 dir = transform.TransformVector(localDir);
            Vector3 dir = transform.parent.transform.TransformVector(localDir);
            RaycastHit hit;
            if (Physics.Raycast(rayStartPos, dir, out hit, maxDist, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.distance - 0.15f;
            }
            return maxDist;
        }

        private void Update()
        {
            if (!LeftHand || !RightHand)
            {
                Destroy(this);
                return;
            }

            PeekForward = ForwardKeyHeldDuration > 1.0f;

            var peekDist = 0f;
            var cameraTargetLocalZ = 0f;
            var cameraTargetLocalX = 0f;

            if (PeekForward)
            {
                Peek = 0;
                peekDist = RayDistance(new Vector3(0f, 0f, 1f), 0.6f);
                cameraTargetLocalZ = peekDist;
            }
            else
            {
                peekDist = Peek != 0 ? RayDistance(new Vector3(Peek, 0f, 0f), 0.6f) : 0f;
                cameraTargetLocalX = peekDist * Peek;
            }

            var targetZRot = -30f * Peek;

            MouseLook.zRotation = Mathf.SmoothDamp(MouseLook.zRotation, targetZRot, ref zRotVelocity, 0.02f, 200.0f);

            Vector3 localPos = CameraTransform.localPosition;
            localPos.x = Mathf.SmoothDamp(localPos.x, cameraTargetLocalX, ref cameraXVelocity, 0.1f, 5f);
            localPos.z = Mathf.SmoothDamp(localPos.z, cameraTargetLocalZ, ref cameraZVelocity, 0.1f, 5f);
            CameraTransform.localPosition = localPos;

            if (attackingTime > 0f)
            {
                attackingTime -= Time.deltaTime;
            }

            bool attackLeft = attackingTime > 0f && attackingTime < AttackDuration - 0.2f;
            bool attackRight = attackingTime > 0.2f;

            Vector3 leftTargetPos = attackLeft ? leftHandOriginal + new Vector3(0f, 0f, WeaponFlyDistance) : leftHandOriginal;
            Vector3 rightTargetPos = attackRight ? rightHandOriginal + new Vector3(0f, 0f, WeaponFlyDistance) : rightHandOriginal;

            Quaternion rot = Quaternion.AngleAxis(60f, Vector3.forward);
            Quaternion leftTargetRot = attackLeft ? rot : leftHandOriginalRot;
            Quaternion rightTargetRot = attackRight ? rot : rightHandOriginalRot;

            LeftHand.transform.localPosition += (leftTargetPos - LeftHand.transform.localPosition) * 20f * Time.deltaTime;
            RightHand.transform.localPosition += (rightTargetPos - RightHand.transform.localPosition) * 20f * Time.deltaTime;

            LeftHand.transform.localRotation = Quaternion.RotateTowards(LeftHand.transform.localRotation, leftTargetRot, 20f);
            RightHand.transform.localRotation = Quaternion.RotateTowards(RightHand.transform.localRotation, rightTargetRot, 20f);
        }
    }
}