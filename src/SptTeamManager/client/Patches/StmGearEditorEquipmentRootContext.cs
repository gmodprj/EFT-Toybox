using EFT.InventoryLogic;
using EFT.UI;

namespace pitTeam.Patches
{
    // STM 장비 편집기의 **장비 트리** 루트 문맥.
    //
    // 왜 PIT 의 LoadoutEditorEquipmentRootContext 를 그대로 쓰지 않는가: 그것은 EmptyItemContext 를
    // 상속하고 CreateChild 가 DefaultItemContext 를 돌려준다. 그런데 EFT 의 메뉴 판정이 문맥의
    // **종류를 직접** 본다(2026-11-03 역컴파일, WORKLOG "스태시 칸의 메뉴가 짧았던 이유"):
    //
    //     case EItemInfoButton.ApplyMagPreset:
    //     case EItemInfoButton.LoadAmmo:
    //         if (!Gameplay && _itemContext is InventorySelectableItemContext) { ... }
    //         return false;
    //
    // 그래서 장비 트리 안의 아이템에는 `삽탄`(LoadAmmo)·`탄창 프리셋`이 **뜨지 않는다** -
    // 2026-11-04 인게임에서 "리그의 탄창에 삽탄이 없다" 로 드러난 것이 이것이다. 스태시 쪽은
    // 2026-11-03 에 이미 같은 이유로 고쳤는데(그쪽은 패널에 문맥 override 를 줬다) 장비 쪽이 남았다.
    //
    // 고치는 법은 **루트부터 그 계열로 두는 것**이다. 자식 생성자
    // `InventorySelectableItemContext(Item, InventorySelectableItemContext)` 가 부모를 요구하므로,
    // 루트가 EmptyItemContext 계열이면 자식에서 부모 고리를 잇지 못한다(어셈블리에서 생성자 목록을
    // 확인했다). 루트를 InventorySelectableItemContext 로 두면 PIT 과 같은 모양으로 고리가 이어지고
    // 종류 판정도 통과한다.
    //
    // 시야 종류도 `Inventory` 로 둔다 - PIT 은 `InventoryDuringMatching` 을 쓰는데, STM 의 요구는
    // "플레이어 자신의 인벤토리 화면과 같게" 이고 그 화면이 쓰는 값이 `Inventory` 다
    // (InventoryScreenController 실측, 스태시 패널에도 같은 값을 준다).
    //
    // **PIT 의 용병 편집기는 건드리지 않는다**(불변식 4). 이 클래스는 STM 편집기만 쓴다.
    internal sealed class StmGearEditorEquipmentRootContext : InventorySelectableItemContext
    {
        public StmGearEditorEquipmentRootContext(EItemViewType viewType)
            : base(viewType)
        {
        }

        public override ItemContext CreateChild(Item item)
        {
            // null 은 PIT 의 루트 문맥과 같게 자기 자신으로 받는다 - 호출부가 null 을 넘길 수 있다.
            if (item == null)
            {
                return this;
            }

            return new InventorySelectableItemContext(item, this);
        }
    }
}
