using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class ItemInspector : MonoBehaviour, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public GameObject RootPanel;
        [SerializeField]
        private RawImage image;
        private Camera cam;
        private RenderTexture renderTexture;
        private GameObject playerPreview;
        private Transform inspectedItem;
        private float pitch;
        private float yaw;
        private float zoom = -3;
        private bool dragging;
        private readonly Vector3 basePosition = new Vector3(0, 200, 1);
        private Vector3 lastMousePosition;

        public void SetCameraClearFlag(int flag)
        {
            cam.clearFlags = (CameraClearFlags)flag;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                lastMousePosition = Input.mousePosition;
                dragging = true;
            }

            pitch = Mathf.Clamp(pitch - (lastMousePosition - Input.mousePosition).y, -80, 80);
            yaw += (lastMousePosition - Input.mousePosition).x;
            cam.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
            cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, 0);
            cam.transform.position = basePosition + cam.transform.forward * zoom;
            lastMousePosition = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
            {
                zoom = Mathf.Clamp(zoom + scroll * GameCamera.instance.m_zoomSens, -10, -1);
                cam.transform.position = basePosition + cam.transform.forward * zoom;
            }
        }

        public void SetInspectedItem(ICirculatedItem item)
        {
            if (inspectedItem != null)
            {
                Destroy(inspectedItem.gameObject);
            }

            if (playerPreview != null)
            {
                playerPreview.SetActive(false);
            }

            GameObject itemPrefab = ZNetScene.instance.GetPrefab(item.Drop.name);

            if (
                item.Drop.m_itemData.IsEquipable() &&
                (item.Drop.gameObject.HasChildWithNameThatContains("attach") || item.Drop.gameObject.HasChildWithNameThatContains("log"))
                )
            {
                Player localPlayer = Player.m_localPlayer;
                playerPreview = playerPreview ?? CreatePlayerPreview();
                Player playerPreviewComp = playerPreview.GetComponent<Player>();
                playerPreviewComp.m_inventory.AddItem(itemPrefab, 1);

                if (playerPreviewComp.m_inventory.m_inventory.Count > 1)
                {
                    playerPreviewComp.m_inventory.m_inventory.RemoveAt(playerPreviewComp.m_inventory.m_inventory.Count - 2);
                }
                
                playerPreviewComp.EquipItem(playerPreviewComp.m_inventory.m_inventory[0]);
                playerPreviewComp.m_visEquipment.SetHairItem(localPlayer.m_hairItem);
                playerPreviewComp.m_visEquipment.SetHairEquipped(localPlayer.m_hairItem.GetStableHashCode());
                playerPreviewComp.m_visEquipment.SetHairColor(localPlayer.m_hairColor);
                playerPreviewComp.m_visEquipment.SetSkinColor(localPlayer.m_skinColor);
                playerPreviewComp.m_visEquipment.SetModel(localPlayer.m_visEquipment.m_currentModelIndex);
                playerPreviewComp.m_visEquipment.UpdateVisuals();
                playerPreviewComp.gameObject.SetActive(true);
                playerPreviewComp.m_animator.SetBool("wakeup", false);
                playerPreviewComp.m_animator.Play("Movement");
                playerPreviewComp.m_animator.Update(100);
                playerPreviewComp.gameObject.SetLayerForEntireHierarchy(LayerMask.NameToLayer("UI"));
                playerPreviewComp.transform.forward = -Vector3.forward;
            }
            else
            {
                ZNetView.m_forceDisableInit = true;
                GameObject randomItem = Instantiate(itemPrefab, basePosition, Quaternion.Euler(Vector3.forward));
                ZNetView.m_forceDisableInit = false;

                if (randomItem.TryGetComponent(out Rigidbody rigi))
                {
                    Destroy(rigi);
                }

                randomItem.SetLayerForEntireHierarchy(LayerMask.NameToLayer("UI"));
                randomItem.name = "Inspected GameObject";
                inspectedItem = randomItem.transform;
            }

            RootPanel.SetActive(true);
        }

        public void UpdateRenderTexture()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
            }

            RectTransform rectTransform = transform as RectTransform;
            renderTexture = new RenderTexture((int)rectTransform.sizeDelta.x, (int)rectTransform.sizeDelta.y, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            cam.targetTexture = renderTexture;
            image.texture = renderTexture;
        }

        private void OnEnable()
        {
            ResetCamera();
        }

        private void OnDisable()
        {
            RootPanel.SetActive(false);
            
            if (playerPreview != null)
            {
                Destroy(playerPreview);
                playerPreview = null;
            }
        }

        private void ResetCamera()
        {
            pitch = 35f;
            yaw = 20f;
            zoom = -3;
            OnDrag(null);
            dragging = false;
        }

        private void Awake()
        {
            var camEmpty = new GameObject("Item Inspector Camera");
            camEmpty.transform.position = basePosition + Vector3.forward * zoom;
            cam = camEmpty.AddComponent<Camera>();
            cam.backgroundColor = Color.gray;
            cam.cullingMask = 1 << LayerMask.NameToLayer("UI");

            var light1 = new GameObject("Item Inspector Light1");
            light1.transform.position = basePosition + new Vector3(1.5f, 0.5f, -2f);
            Light light1Comp = light1.AddComponent<Light>();
            light1Comp.type = LightType.Point;
            light1Comp.intensity = 3.5f;
            light1Comp.range = 3;

            var light2 = new GameObject("Item Inspector Light2");
            light2.transform.position = basePosition + new Vector3(-1.5f, -0.5f, 1.5f);
            Light light2Comp = light2.AddComponent<Light>();
            light2Comp.type = LightType.Point;
            light2Comp.intensity = 2.5f;
            light2Comp.range = 3;
            UpdateRenderTexture();
        }

        private GameObject CreatePlayerPreview()
        {
            ZNetView.m_forceDisableInit = true;
            GameObject newPlayerPreview = Instantiate(Player.m_localPlayer.gameObject, basePosition - Vector3.up, Quaternion.identity);
            Player.s_players.Remove(newPlayerPreview.GetComponent<Player>());
            newPlayerPreview.GetComponent<PlayerController>().enabled = false;
            newPlayerPreview.GetComponent<ZSyncTransform>().enabled = false;
            newPlayerPreview.GetComponent<ZSyncAnimation>().enabled = false;
            newPlayerPreview.GetComponent<FootStep>().enabled = false;
            newPlayerPreview.GetComponent<Rigidbody>().isKinematic = true;
            newPlayerPreview.gameObject.SetActive(false);
            newPlayerPreview.gameObject.name = "Item Inspector Player Preview";
            newPlayerPreview.gameObject.SetLayerForEntireHierarchy(LayerMask.NameToLayer("UI"));
            ZNetView.m_forceDisableInit = false;

            return newPlayerPreview;
        }
    }
}
