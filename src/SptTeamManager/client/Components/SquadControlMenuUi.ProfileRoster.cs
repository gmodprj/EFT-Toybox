using EFT.UI;
using pitTeam.Modules;
using pitTeam.Shared;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pitTeam.Components
{
    // STM: launcher-profile teammates on the squad roster (design 6-3, left list).
    //
    // Everything here sits beside PIT's mercenary roster and never replaces it: the mercenary tiles
    // are built by the untouched BuildRosterEntries/CreateRosterTile path, and these tiles are added
    // after them. Nothing in this file stores a roster; each rebuild asks the server again.
    internal partial class SquadControlMenuUi
    {
        private const float StmPickerRowWidth = 520f;
        private const float StmPickerRowHeight = 44f;
        private const float StmPickerRowStep = 52f;
        private const int StmPickerMaxRows = 8;
        private const float StmLoadButtonOffsetX = 340f;

        private DefaultUIButton loadProfileButton;
        private GameObject stmPickerOverlay;
        private bool stmRequestBusy;

        // Fetched on every roster rebuild. null = the profile section is not shown at all.
        private static StmRosterSnapshot LoadStmSnapshot()
        {
            // Profile members are managed the same in every gameplay mode (the registration store and
            // the friend-list merge are mode-independent); there is deliberately no mode gate here.
            try
            {
                return StmProfileRosterClient.Load();
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Profile roster load threw unexpectedly: {ex.Message}");
                return StmRosterWire.Failed(ex.Message);
            }
        }

        // A failed load is drawn as one tile, so it counts as one.
        private static int CountStmTiles(StmRosterSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            if (!snapshot.Ok)
            {
                return 1;
            }

            // One extra notice tile when members are shown but the session check could not be completed.
            return snapshot.Members.Count + (NeedsSessionNotice(snapshot) ? 1 : 0);
        }

        private static bool NeedsSessionNotice(StmRosterSnapshot snapshot)
        {
            return snapshot != null && snapshot.Ok && !snapshot.OwnAccountKnown && snapshot.Members.Count > 0;
        }

        // Contained: if drawing these tiles fails, PIT's tiles and layout (built by the caller) stand.
        private void AppendStmTiles(RectTransform parent, StmRosterSnapshot snapshot)
        {
            if (parent == null || snapshot == null)
            {
                return;
            }

            try
            {
                if (!snapshot.Ok)
                {
                    CreateStmTile(
                        parent,
                        "loadfailed",
                        StmProfileRosterClient.LoadFailedTitle,
                        string.Empty,
                        StmProfileRosterClient.LoadFailedLine,
                        unavailable: true,
                        removeProfileId: null,
                        openAccountId: null);
                    return;
                }

                foreach (RegisteredProfileRow row in snapshot.Members)
                {
                    // A non-Ready profile is drawn greyed with its reason and has no click handler of
                    // its own, so it cannot be selected for anything. Its only control is "remove".
                    bool ready = row.Status == RosterRowStatus.Ready;
                    CreateStmTile(
                        parent,
                        row.ProfileId,
                        string.IsNullOrWhiteSpace(row.Nickname) ? row.ProfileId : row.Nickname,
                        ready ? $"Lv {row.Level}  {row.Side}" : string.Empty,
                        StmStatusLine(row.Status),
                        unavailable: !ready,
                        removeProfileId: row.ProfileId,
                        openAccountId: StmTileAccountId(row, ready));
                }

                // Rows stay visible; the user is told the client-side session filter did not run.
                if (NeedsSessionNotice(snapshot))
                {
                    CreateStmTile(
                        parent,
                        "sessionunverified",
                        StmProfileRosterClient.SessionUnverifiedTitle,
                        string.Empty,
                        StmProfileRosterClient.SessionUnverifiedLine,
                        unavailable: true,
                        removeProfileId: null,
                        openAccountId: null);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Failed to draw profile teammate tiles; the mercenary roster is unaffected.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        // The account id every profile route is keyed by is the aid as a string (StmRosterWire.IsCurrentSession
        // compares it the same way). A row without a usable aid - a Missing registration has aid 0 - cannot be
        // opened: SPT's GetOtherProfile would fall back to the player's own profile for an unknown account id.
        private static string StmTileAccountId(RegisteredProfileRow row, bool ready)
        {
            return ready && row.Aid != 0 ? row.Aid.ToString() : null;
        }

        // One line per status. CurrentSession never reaches here (filtered in StmProfileRosterClient.Load);
        // if it ever did it would read as unreadable, never as usable.
        private static string StmStatusLine(RosterRowStatus status)
        {
            switch (status)
            {
                case RosterRowStatus.Ready:
                    return StmProfileRosterClient.EquipmentUndecided;
                case RosterRowStatus.Empty:
                    return StmProfileRosterClient.ReasonEmpty;
                case RosterRowStatus.Missing:
                    return StmProfileRosterClient.ReasonMissing;
                default:
                    return StmProfileRosterClient.ReasonUnreadable;
            }
        }

        private GameObject CreateStmTile(
            RectTransform parent,
            string key,
            string title,
            string infoLine,
            string statusLine,
            bool unavailable,
            string removeProfileId,
            string openAccountId)
        {
            GameObject tileObject = new GameObject(
                $"pitFireTeam_StmTile_{key}",
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement));
            tileObject.transform.SetParent(parent, false);

            RectTransform tileRect = tileObject.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(RosterTileWidth, RosterTileHeight);

            LayoutElement layoutElement = tileObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = RosterTileWidth;
            layoutElement.preferredHeight = RosterTileHeight;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            Image tileBackground = tileObject.GetComponent<Image>();
            tileBackground.color = unavailable
                ? new Color(0.045f, 0.045f, 0.045f, 0.55f)
                : new Color(0.045f, 0.045f, 0.045f, 0.97f);

            Color nameColor = unavailable ? new Color(0.5f, 0.5f, 0.5f, 1f) : new Color(0.86f, 0.86f, 0.84f, 1f);
            Color infoColor = unavailable ? new Color(0.45f, 0.45f, 0.45f, 1f) : new Color(0.7f, 0.7f, 0.68f, 1f);
            Color statusColor = unavailable ? new Color(0.85f, 0.7f, 0.45f, 1f) : new Color(0.6f, 0.6f, 0.58f, 1f);

            AddStmTileText(tileRect, "Caption", StmProfileRosterClient.TileCaption, 16f,
                new Color(0.55f, 0.55f, 0.53f, 1f), new Vector2(16f, 176f), new Vector2(-44f, -10f));
            TextMeshProUGUI nameLabel = AddStmTileText(tileRect, "Name", title, 24f, nameColor, new Vector2(16f, 114f), new Vector2(-16f, -44f));
            AddStmTileText(tileRect, "Info", infoLine, 20f, infoColor, new Vector2(16f, 84f), new Vector2(-16f, -100f));
            AddStmTileText(tileRect, "Status", statusLine, 18f, statusColor, new Vector2(16f, 14f), new Vector2(-16f, -134f));

            if (!string.IsNullOrWhiteSpace(removeProfileId))
            {
                Button removeButton = CreateWindowCloseButton(tileRect, "pitFireTeam_StmRemoveButton");
                if (removeButton.transform is RectTransform removeRect)
                {
                    removeRect.anchorMin = new Vector2(1f, 1f);
                    removeRect.anchorMax = new Vector2(1f, 1f);
                    removeRect.pivot = new Vector2(1f, 1f);
                    removeRect.anchoredPosition = new Vector2(-6f, -6f);
                }

                string profileId = removeProfileId;
                removeButton.onClick.AddListener(() => RemoveStmProfile(profileId));
            }

            // The way in to the teammate detail screen (and from there the gear editor). Same construction as
            // the mercenary tile (SquadControlMenuUi.Roster.cs CreateRosterTile): a Button on the tile itself
            // plus the shared hover controller. The remove button is a child Button and consumes its own click,
            // so it does not open the screen. An unavailable row is passed no account id and stays inert.
            if (!string.IsNullOrWhiteSpace(openAccountId))
            {
                Color hoverColor = new Color(0.6235f, 0.6157f, 0.5647f, 1f);
                Color pressedColor = new Color(0.16f, 0.16f, 0.16f, 0.99f);

                Button tileButton = tileObject.AddComponent<Button>();
                tileButton.transition = Selectable.Transition.None;
                tileButton.targetGraphic = tileBackground;

                string accountId = openAccountId;
                tileButton.onClick.AddListener(() => OpenProfile(accountId));

                RosterTileHoverController hoverController = tileObject.AddComponent<RosterTileHoverController>();
                hoverController.Configure(tileBackground, nameLabel, tileBackground.color, hoverColor, pressedColor);
            }

            return tileObject;
        }

        private TextMeshProUGUI AddStmTileText(
            RectTransform tileRect,
            string name,
            string text,
            float fontSize,
            Color color,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject textObject = CreateText($"pitFireTeam_Stm{name}", text ?? string.Empty, fontSize, TextAlignmentOptions.TopLeft);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(tileRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = offsetMin;
            textRect.offsetMax = offsetMax;

            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        // ---- "프로필 불러오기" ---------------------------------------------------------------

        // Same construction as CreateAddTeammateButton (Roster.cs). Position is set in UpdateRosterPanelLayout.
        private void TryCreateLoadProfileButton(RectTransform rosterRect, DefaultUIButton buttonTemplate)
        {
            try
            {
                if (loadProfileButton != null)
                {
                    Destroy(loadProfileButton.gameObject);
                    loadProfileButton = null;
                }

                if (rosterRect == null || buttonTemplate == null || playerButton == null)
                {
                    return;
                }

                loadProfileButton = Instantiate(buttonTemplate, rosterRect, false);
                loadProfileButton.name = "pitFireTeam_SquadControlLoadProfileButton";
                loadProfileButton.SetRawText(StmProfileRosterClient.LoadButtonText, playerButton.HeaderSize);
                loadProfileButton.SetIcon(null);
                loadProfileButton.Interactable = true;
                loadProfileButton.OnClick.RemoveAllListeners();
                loadProfileButton.OnClick.AddListener(OnLoadProfileClicked);
                loadProfileButton.gameObject.SetActive(true);

                RectTransform buttonRect = loadProfileButton.transform as RectTransform;
                if (buttonRect != null)
                {
                    buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                    buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                    buttonRect.pivot = new Vector2(0.5f, 0.5f);
                    buttonRect.sizeDelta = new Vector2(320f, playerButton.transform is RectTransform playerRect ? playerRect.sizeDelta.y : 54f);
                    buttonRect.anchoredPosition = new Vector2(StmLoadButtonOffsetX, 0f);
                }
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] Failed to create the load-profile button; the add-teammate button is unaffected.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        private void OnLoadProfileClicked()
        {
            if (GameplayModeRuntime.IsSwitching || stmRequestBusy)
            {
                return;
            }

            // A fresh read every time the picker opens.
            ShowStmPicker(LoadStmSnapshot());
        }

        private void ShowStmPicker(StmRosterSnapshot snapshot)
        {
            CloseStmPicker();

            Transform overlayParent = rosterPanel?.transform.parent;
            if (overlayParent == null || snapshot == null)
            {
                return;
            }

            string message = null;
            if (!snapshot.Ok)
            {
                message = StmProfileRosterClient.PickerFailedLine;
            }
            else if (!snapshot.OwnAccountKnown)
            {
                message = StmProfileRosterClient.PickerOwnUnknownLine;
            }
            else if (snapshot.Candidates.Count == 0)
            {
                message = StmProfileRosterClient.PickerNoneLine;
            }

            int rowCount = message == null ? Mathf.Min(snapshot.Candidates.Count, StmPickerMaxRows) : 0;
            int hiddenCount = message == null ? snapshot.Candidates.Count - rowCount : 0;
            float moreLineHeight = hiddenCount > 0 ? 34f : 0f;

            GameObject overlayRoot = new GameObject("pitFireTeam_StmPickerOverlay", typeof(RectTransform), typeof(Image));
            overlayRoot.transform.SetParent(overlayParent, false);
            RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
            Stretch(overlayRect);
            overlayRect.SetAsLastSibling();

            Image backdrop = overlayRoot.GetComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.12f);
            backdrop.raycastTarget = true;

            GameObject panel = new GameObject("pitFireTeam_StmPickerPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620f, message == null ? 60f + rowCount * StmPickerRowStep + moreLineHeight : 200f);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);
            panelImage.raycastTarget = true;

            GameObject header = new GameObject("pitFireTeam_StmPickerHeader", typeof(RectTransform), typeof(Image));
            header.transform.SetParent(panel.transform, false);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.offsetMin = new Vector2(0f, -28f);
            headerRect.offsetMax = new Vector2(0f, 0f);

            Image headerImage = header.GetComponent<Image>();
            headerImage.color = new Color(0.06f, 0.06f, 0.06f, 1f);
            headerImage.raycastTarget = true;

            GameObject titleObject = CreateText(
                "pitFireTeam_StmPickerTitle",
                StmProfileRosterClient.PickerTitle,
                18f,
                TextAlignmentOptions.MidlineLeft);
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.SetParent(header.transform, false);
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(16f, 0f);
            titleRect.offsetMax = new Vector2(-42f, 0f);

            Button closeButton = CreateWindowCloseButton(header.transform, "pitFireTeam_StmPickerCloseButton");
            if (closeButton.transform is RectTransform closeRect)
            {
                closeRect.anchorMin = new Vector2(1f, 0.5f);
                closeRect.anchorMax = new Vector2(1f, 0.5f);
                closeRect.pivot = new Vector2(1f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-4f, 0f);
            }

            closeButton.onClick.AddListener(CloseStmPicker);

            if (message != null)
            {
                GameObject bodyObject = CreateText("pitFireTeam_StmPickerBody", message, 22f, TextAlignmentOptions.Center);
                RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
                bodyRect.SetParent(panel.transform, false);
                bodyRect.anchorMin = new Vector2(0f, 0f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.offsetMin = new Vector2(28f, 20f);
                bodyRect.offsetMax = new Vector2(-28f, -42f);

                TextMeshProUGUI bodyLabel = bodyObject.GetComponent<TextMeshProUGUI>();
                bodyLabel.enableWordWrapping = true;
                bodyLabel.overflowMode = TextOverflowModes.Ellipsis;
            }
            else
            {
                // Only Ready, unregistered, non-current-session rows reach this list
                // (StmProfileRosterClient.Load); a non-Ready profile has no row to click.
                for (int i = 0; i < rowCount; i++)
                {
                    RegisteredProfileRow candidate = snapshot.Candidates[i];
                    float y = 12f + moreLineHeight + (rowCount - 1 - i) * StmPickerRowStep;
                    DefaultUIButton rowButton = CreateOverlayActionButton(
                        panel.transform,
                        new Vector2(0f, y),
                        new Vector2(StmPickerRowWidth, StmPickerRowHeight));
                    rowButton.SetRawText($"{candidate.Nickname}   Lv {candidate.Level}   {candidate.Side}", 22);
                    rowButton.OnClick.RemoveAllListeners();
                    rowButton.OnClick.AddListener(() => AddStmProfile(candidate));
                }

                if (hiddenCount > 0)
                {
                    GameObject moreObject = CreateText(
                        "pitFireTeam_StmPickerMore",
                        string.Format(StmProfileRosterClient.PickerMoreLineFormat, hiddenCount),
                        18f,
                        TextAlignmentOptions.Center);
                    RectTransform moreRect = moreObject.GetComponent<RectTransform>();
                    moreRect.SetParent(panel.transform, false);
                    moreRect.anchorMin = new Vector2(0f, 0f);
                    moreRect.anchorMax = new Vector2(1f, 0f);
                    moreRect.pivot = new Vector2(0.5f, 0f);
                    moreRect.offsetMin = new Vector2(20f, 4f);
                    moreRect.offsetMax = new Vector2(-20f, 4f + moreLineHeight);
                }
            }

            stmPickerOverlay = overlayRoot;
        }

        private void CloseStmPicker()
        {
            if (stmPickerOverlay == null)
            {
                return;
            }

            Destroy(stmPickerOverlay);
            stmPickerOverlay = null;
        }

        private async void AddStmProfile(RegisteredProfileRow row)
        {
            if (row == null || stmRequestBusy)
            {
                return;
            }

            // Checked again here, right before the call, whatever the picker was built from.
            if (row.Status != RosterRowStatus.Ready || string.IsNullOrWhiteSpace(row.ProfileId))
            {
                return;
            }

            stmRequestBusy = true;
            try
            {
                string failure = await StmProfileRosterClient.AddAsync(row.ProfileId);
                if (this == null)
                {
                    return;
                }

                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                CloseStmPicker();
                StmProfileRosterClient.RefreshNativeFriendList();
                RebuildRosterTiles();
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError($"[STM] Adding profile '{row.ProfileId}' failed.");
                pitFireTeam.Log.LogError(ex);
                AddTeammateCreationFlow.ShowToast(StmProfileRosterClient.AddFailedToast);
            }
            finally
            {
                stmRequestBusy = false;
            }
        }

        private async void RemoveStmProfile(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId) || stmRequestBusy)
            {
                return;
            }

            stmRequestBusy = true;
            try
            {
                string failure = await StmProfileRosterClient.RemoveAsync(profileId);
                if (this == null)
                {
                    return;
                }

                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmProfileRosterClient.RefreshNativeFriendList();
                RebuildRosterTiles();
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError($"[STM] Removing profile '{profileId}' failed.");
                pitFireTeam.Log.LogError(ex);
                AddTeammateCreationFlow.ShowToast(StmProfileRosterClient.RemoveFailedToast);
            }
            finally
            {
                stmRequestBusy = false;
            }
        }
    }
}
