using MelonLoader;
using Il2CppSLZ.Marrow;
using BoneLib;
using BoneLib.BoneMenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HD_Camera
{
    public class HDCamera : MelonMod
    {
        public enum ImageSize
        {
            Small = 256,
            Medium = 512,
            Large = 1024,
            Ultra = 2160,
            Insane = 4320
        };

        //private IntElement m_size;
        private EnumElement m_size;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            InitializeMenu();
            LoggerInstance.Msg("Initialized.");
        }

        private void InitializeMenu()
        {
            Page page = Page.Root.CreatePage("HDCamera", Color.green);
            m_size = new EnumElement("size", Color.white, ImageSize.Large, null);
            page.Add(m_size);
            //m_size = page.CreateInt("Size (width and height)", Color.white, 256, 16, 256, 2560, null);
            page.CreateFunction("Apply to held camera", Color.white, ApplyToHeldCamera);
        }

        private void ApplyToHeldCamera()
        {
            (Transform toplevelL, Camera l) = FindHeldCamera(Player.PhysicsRig.leftHand);
            (Transform toplevelR, Camera r) = FindHeldCamera(Player.PhysicsRig.rightHand);

            ImageSize imageSize = (ImageSize)m_size.Value;

            if (l != null)
            {                
                toplevelL.Find("Variables/width").GetComponent<EventSystem>().m_DragThreshold = (int)imageSize;
                l.targetTexture.Release(); //Enum: R8G8B8A8_UNORM
                l.targetTexture = new RenderTexture((int)imageSize, (int)imageSize, 0, RenderTextureFormat.ARGB64);
                l.targetTexture.Create();
            }
            else if (r != null)
            {
                toplevelR.Find("Variables/width").GetComponent<EventSystem>().m_DragThreshold = (int)imageSize;
                r.targetTexture.Release();
                r.targetTexture = new RenderTexture((int)imageSize, (int)imageSize, 0, RenderTextureFormat.ARGB64);
                r.targetTexture.Create();
            }
            else
            {
                LoggerInstance.Msg("Didnt detect valid camera in either hand");
            }
        }

        private (Transform, Camera) FindHeldCamera(Hand hand)
        {
            LoggerInstance.Msg($"Checking {hand.name} hand");

            if (hand.AttachedReceiver == null)
            {
                LoggerInstance.Msg("Hand is not holding anything");
                return (null, null);
            }

            InteractableHost componentInParent = hand.AttachedReceiver.gameObject.GetComponentInParent<InteractableHost>(true);
            if (componentInParent == null)
            {
                LoggerInstance.Msg("Could not find a InteractableHost");
                return (null, null);
            }

            Transform transform1 = componentInParent.transform;
            if (transform1.Find("IS_CAMERA") == null) // held object is not a camera
            {
                LoggerInstance.Msg("Held object is not a camera");
                return (transform1, null);
            }

            Transform camTransform = transform1.Find("CameraTarget/rt");
            if (camTransform == null) // cant find the camera
            {
                LoggerInstance.Msg("Cant find camera component");
                return (transform1, null);
            }

            return (transform1, camTransform.GetComponent<Camera>());
        }
    }
}
