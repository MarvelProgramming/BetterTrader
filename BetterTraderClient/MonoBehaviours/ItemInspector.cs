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
        [SerializeField]
        private RawImage image;
        private Camera cam;
        private GameObject playerPreview;
        private Transform inspectedItem;
        private float pitch;
        private float yaw;
        private float zoom = -3;
        private bool dragging;
        private readonly Vector3 basePosition = new Vector3(0, 200, 1);
        private Vector3 lastMousePosition;

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
            transform.parent.gameObject.SetActive(true);
            ResetCamera();

            if (inspectedItem != null)
            {
                Destroy(inspectedItem.gameObject);
            }

            playerPreview.SetActive(false);
            GameObject itemPrefab = ZNetScene.instance.GetPrefab(item.Drop.name);

            if (item.Drop.m_itemData.IsEquipable() && item.Drop.m_itemData.m_shared.m_ammoType.Length == 0)
            {
                Player playerPreviewComp = playerPreview.GetComponent<Player>();
                playerPreviewComp.UnequipAllItems();
                playerPreviewComp.m_inventory.m_inventory.Clear();
                playerPreviewComp.m_inventory.AddItem(itemPrefab, 1);

                foreach (ItemDrop.ItemData equippedLocalPlayerItem in Player.m_localPlayer.m_inventory.GetEquippedItems())
                {
                    playerPreviewComp.m_inventory.AddItem(equippedLocalPlayerItem.m_dropPrefab, 1);
                    playerPreviewComp.EquipItem(playerPreviewComp.m_inventory.m_inventory[playerPreviewComp.m_inventory.m_inventory.Count - 1]);
                }

                playerPreviewComp.EquipItem(playerPreviewComp.m_inventory.m_inventory[0]);
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

            transform.parent.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            transform.parent.gameObject.SetActive(false);
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
            cam.cullingMask = 1 << LayerMask.NameToLayer("UI");

            var camRt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            camRt.Create();
            cam.targetTexture = camRt;
            image.texture = camRt;

            ZNetView.m_forceDisableInit = true;
            playerPreview = Instantiate(Game.instance.m_playerPrefab, basePosition - Vector3.up, Quaternion.identity);
            Player.s_players.Remove(playerPreview.GetComponent<Player>());
            playerPreview.GetComponent<PlayerController>().enabled = false;
            playerPreview.GetComponent<ZSyncTransform>().enabled = false;
            playerPreview.GetComponent<ZSyncAnimation>().enabled = false;
            playerPreview.GetComponent<FootStep>().enabled = false;
            playerPreview.GetComponent<Rigidbody>().isKinematic = true;
            playerPreview.gameObject.SetActive(false);
            playerPreview.gameObject.name = "Item Inspector Player Preview";
            playerPreview.gameObject.SetLayerForEntireHierarchy(LayerMask.NameToLayer("UI"));
            ZNetView.m_forceDisableInit = false;
        }
    }
}
