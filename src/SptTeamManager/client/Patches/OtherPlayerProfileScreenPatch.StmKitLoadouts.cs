using EFT.UI;
using Newtonsoft.Json;
using pitTeam.Modules;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ResultProfile = EFT.OtherPlayerProfile;

namespace pitTeam.Patches
{
    // 서버가 /stm/members/builds 로 주는 저장된 장비 빌드 목록. **두 묶음이 각자의 자리에 온다** -
    // 한 목록에 꼬리표로 섞여 오지 않으므로 어느 쪽 빌드인지 헷갈릴 여지가 없다.
    internal class StmGearBuildRowWire
    {
        public string Id { get; set; }
        public string Name { get; set; }
        // 팀원 빌드에만 온다. 게임의 기본 장비 프리셋 화면에 보이려면 클라이언트가 EquipmentBuild 를 직접
        // 만들어야 하고, 그 생성자가 (id, name, root, FlatItem[], buildType) 를 받는다(역컴파일로 확인).
        // 이 팀원에게 입히는 데 드는 값. -1 이면 값을 매길 수 없다(살 수 없는 빌드다).
        public int Price { get; set; } = -1;
        public string Root { get; set; }
        public JsonType.FlatItem[] Items { get; set; }
    }

    internal class StmGearBuildsWire
    {
        public List<StmGearBuildRowWire> TeammateBuilds { get; set; }
        public List<StmGearBuildRowWire> PlayerBuilds { get; set; }
        public string TeammateNickname { get; set; }
        public string ProfileId { get; set; }
    }

    internal class StmKitSlotWire
    {
        public string SlotId { get; set; }
        public string Outcome { get; set; }
        public int BuyCount { get; set; }
        public int StashCount { get; set; }
    }

    internal class StmKitQuoteWire
    {
        public string BuildId { get; set; }
        public string BuildName { get; set; }
        public string Owner { get; set; }
        public int Total { get; set; }
        public bool Affordable { get; set; }
        public int PlayerRoubles { get; set; }
        public int ItemsToBuy { get; set; }
        public int ItemsToStash { get; set; }
        public List<StmKitSlotWire> Slots { get; set; }
    }

    internal class StmKitBuyWire
    {
        public string BuildName { get; set; }
        public int Paid { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsStashed { get; set; }
    }

    // 계획 4 Task 4 — 프로필 팀원의 장비 꾸러미로 가는 **입구 두 개**.
    //
    // 둘 다 게임의 기본 장비 프리셋 화면(용병의 장비 꾸러미와 같은 화면)을 연다. 다른 것은 그 화면에
    // 실리는 빌드가 누구 것이냐뿐이다:
    //   · 내 장비 꾸러미   - 화면이 평소대로 플레이어의 보관소를 읽는다.
    //   · 팀원 장비 꾸러미 - 서버에서 그 팀원의 빌드를 아이템까지 받아 와 화면에 싣는다(StmKitNativeFlow).
    //
    // 값 매기기·확인·구매는 그 화면 위에서 StmKitNativeFlow 가 한다. 이 파일은 입구와 전선 모양만 갖는다.
    // (2026-11-02: 자체 목록 창을 먼저 만들었다가 걷어냈다 - 두 입구가 모두 기본 화면을 열게 되면서
    //  닿지 않는 코드가 됐고, 닿지 않는 코드는 남겨 두지 않는다.)
    internal partial class OtherPlayerProfileScreenPatch
    {
        private const string StmBuildsRoute = "/stm/members/builds";

        private const string StmKitButtonText = "내 장비 꾸러미";
        private const string StmKitTeammateButtonText = "팀원 장비 꾸러미";
        private const string StmKitLoadFailedToast = "빌드 목록을 불러오지 못했습니다";
        private const string StmKitNoBuildsText = "이 팀원의 프로필에 저장된 장비 빌드가 없습니다. 그 프로필로 로그인해 장비 프리셋을 먼저 저장하세요.";

        private static RectTransform StmKitButtonRoot;
        private static RectTransform StmKitTeammateButtonRoot;
        private static bool StmKitBusy;

        // ---- 입구 버튼 두 줄 ----

        private static void CreateStmKitButton(
            OtherPlayerProfileScreen screen,
            RectTransform loadoutSelector,
            Transform parent,
            ResultProfile profile,
            StmProfileDetailWire detail,
            EFT.InventoryLogic.InventoryController inventoryController,
            EFT.IEftSession session)
        {
            try
            {
                if (screen == null || loadoutSelector == null || parent == null || profile == null) return;

                StmKitButtonRoot = CreateStmKitRowButton(
                    screen, loadoutSelector, parent, detail,
                    StmKitButtonRoot, rowOffset: 2,
                    name: "pitFireTeam_StmKitButton",
                    text: StmKitButtonText,
                    onClick: () => OpenStmKitScreen(profile, detail, teammateOwned: false,
                        inventoryController: inventoryController, session: session));

                StmKitTeammateButtonRoot = CreateStmKitRowButton(
                    screen, loadoutSelector, parent, detail,
                    StmKitTeammateButtonRoot, rowOffset: 3,
                    name: "pitFireTeam_StmTeammateKitButton",
                    text: StmKitTeammateButtonText,
                    onClick: () => OpenStmKitScreen(profile, detail, teammateOwned: true,
                        inventoryController: inventoryController, session: session));
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogError("[STM] 장비 꾸러미 버튼을 만들다 실패했다. 나머지 화면은 그대로다.");
                pitFireTeam.Log.LogError(ex);
            }
        }

        private static RectTransform CreateStmKitRowButton(
            OtherPlayerProfileScreen screen,
            RectTransform loadoutSelector,
            Transform parent,
            StmProfileDetailWire detail,
            RectTransform existing,
            int rowOffset,
            string name,
            string text,
            Action onClick)
        {
            if (existing != null)
            {
                GameObject.Destroy(existing.gameObject);
            }

            RectTransform rowClone = GameObject.Instantiate(loadoutSelector, parent, true);
            rowClone.name = name + "Row";
            rowClone.anchoredPosition = loadoutSelector.anchoredPosition + GetProfileControlRowOffset(loadoutSelector, rowOffset);
            rowClone.gameObject.SetActive(true);

            // 복제한 줄의 드롭다운 두 칸은 쓰지 않는다 - 버튼 하나만 올린다.
            foreach (var part in new[] { "Upper", "Lower" })
            {
                Transform child = rowClone.Find(part);
                if (child != null) child.gameObject.SetActive(false);
            }

            DefaultUIButton buttonTemplate = HideoutButtonField?.GetValue(screen) as DefaultUIButton;
            if (buttonTemplate == null)
            {
                GameObject.Destroy(rowClone.gameObject);
                pitFireTeam.Log.LogWarning($"[STM] '{text}' 버튼을 만들지 못했다: 버튼 템플릿이 없다.");
                return null;
            }

            DefaultUIButton button = GameObject.Instantiate(buttonTemplate, rowClone, false);
            button.name = name;
            button.gameObject.SetActive(true);
            button.SetRawText(text, 18);
            HideProfileButtonIconContainer(button);
            // 장비 편집이 막혀 있으면 꾸러미도 막힌다: 서버가 같은 관문(레이드·미결 전송)을 선다.
            button.Interactable = detail != null && detail.CanEditGear;
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(() => onClick());

            if (button.transform is RectTransform buttonRect && buttonTemplate.transform is RectTransform templateRect)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = Vector2.zero;
                buttonRect.sizeDelta = templateRect.sizeDelta;
                buttonRect.localScale = Vector3.one;
            }

            return rowClone;
        }

        private static void CloseStmKitButtons()
        {
            if (StmKitButtonRoot != null)
            {
                GameObject.Destroy(StmKitButtonRoot.gameObject);
                StmKitButtonRoot = null;
            }

            if (StmKitTeammateButtonRoot != null)
            {
                GameObject.Destroy(StmKitTeammateButtonRoot.gameObject);
                StmKitTeammateButtonRoot = null;
            }
        }

        // ---- 화면 열기 ----

        // 게임의 기본 장비 프리셋 화면을 연다. STM 모드를 먼저 켜고 PIT 의 흐름에 넘긴다 - 그 안에서
        // 값과 구매만 STM 의 경로로 갈라진다(StmKitNativeFlow).
        //
        // **두 입구가 모두 목록을 먼저 받는다.** 전에는 "내 꾸러미" 가 목록 없이 바로 열렸는데,
        // 그러면 줄에 보여 줄 값이 없어 무게가 그대로 남았다(2026-11-03 사용자 지적). 값은 팀원이
        // 이미 입고 있는 칸을 빼고 계산하므로 서버만 알 수 있고, 그것이 구매 창이 청구할 값이다.
        private static async void OpenStmKitScreen(
            ResultProfile profile,
            StmProfileDetailWire detail,
            bool teammateOwned,
            EFT.InventoryLogic.InventoryController inventoryController,
            EFT.IEftSession session)
        {
            if (StmKitBusy || profile == null || detail == null) return;
            StmKitBusy = true;
            try
            {
                string body = JsonConvert.SerializeObject(new { aid = profile.AccountId, profileId = detail.ProfileId });
                string json = await Task.Run(() => RequestHandler.PostJson(StmBuildsRoute, body));

                string failure = ReadStmCloseFailure(json, StmKitLoadFailedToast);
                if (failure != null)
                {
                    AddTeammateCreationFlow.ShowToast(failure);
                    return;
                }

                StmGearBuildsWire builds = DeserializeBodySuccess<StmGearBuildsWire>(json)?.data;
                var owned = teammateOwned ? builds?.TeammateBuilds : builds?.PlayerBuilds;
                var rows = owned?.Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id)).ToArray();
                if (rows == null || rows.Length == 0)
                {
                    AddTeammateCreationFlow.ShowToast(StmKitNoBuildsText);
                    return;
                }

                // 팀원 빌드만 화면에 실어 준다(플레이어 빌드는 게임이 이미 갖고 있다). 실을 것은
                // 아이템이 함께 온 줄뿐이다 - 아이템 없이 끼우면 빈 빌드가 보인다.
                var toLoad = teammateOwned
                    ? rows.Where(r => r.Items != null && r.Items.Length > 0).ToArray()
                    : null;
                if (teammateOwned && (toLoad == null || toLoad.Length == 0))
                {
                    AddTeammateCreationFlow.ShowToast(StmKitNoBuildsText);
                    return;
                }

                // 전역(ActiveProfile*)에 기대지 않고 상세 화면이 받은 값을 그대로 쓴다 - 그 전역은
                // PIT 의 용병 분기에서 채워지는 것이라 STM 경로에서는 비어 있을 수 있다.
                var useSession = session ?? ActiveProfileSession;
                var useController = inventoryController ?? ActiveProfileInventoryController;
                if (useSession == null || useController == null)
                {
                    pitFireTeam.Log.LogWarning("[STM] 장비 꾸러미를 열지 못했다: 세션이나 인벤토리 컨트롤러가 없다.");
                    AddTeammateCreationFlow.ShowToast(StmKitLoadFailedToast);
                    return;
                }

                StmKitNativeFlow.Begin(profile, detail, toLoad, useSession, rows);
                TeammateEquipmentBuildsScreenFlow.Open(profile, useSession, useController);
            }
            catch (Exception ex)
            {
                StmKitNativeFlow.End();
                pitFireTeam.Log.LogError("[STM] 기본 장비 프리셋 화면을 열지 못했다.");
                pitFireTeam.Log.LogError(ex);
                AddTeammateCreationFlow.ShowToast(StmKitLoadFailedToast);
            }
            finally
            {
                StmKitBusy = false;
            }
        }

    }
}
