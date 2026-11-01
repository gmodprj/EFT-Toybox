namespace pitTeam.Shared
{
    // StripWornGear = delete the gear the teammate wore into the raid from the stored profile.
    public enum OutcomeAction { Nothing, StripWornGear }

    // Reason is one human-readable line; it goes into logs and player-facing messages.
    public sealed record OutcomePlan(OutcomeAction Action, string Reason);

    // Decides what a finished raid costs a profile teammate. Pure: no SPT types, no profile access -
    // a later task performs the deletion this decides.
    public static class RaidOutcomePolicy
    {
        // Deliberately has NO catch-all: nothing here can throw, and a catch would make the guards
        // unobservable. Rules are checked in this order, and the order is what decides the Reason when
        // several rules would each say Nothing:
        //   1. already applied  2. no gear record  3. temporary gear  4. escaped  5. strip (Real only)  6. unknown kind.
        // Only rule 5 deletes anything, and only on an explicit Real match; every other path fails closed to Nothing.
        public static OutcomePlan Decide(bool escaped, SpawnGearKind? gearKind, bool alreadyApplied)
        {
            // Idempotence (design 6-8): a key that was already applied must never strip twice.
            if (alreadyApplied)
            {
                return new OutcomePlan(OutcomeAction.Nothing, "이미 적용된 결과라 다시 처리하지 않는다.");
            }

            // No record of what the teammate wore (a server restart between spawn and raid end, say).
            // Deleting a real profile's equipment on a guess is the worst outcome, so no record means no deletion.
            if (gearKind == null)
            {
                return new OutcomePlan(OutcomeAction.Nothing, "어떤 장비로 나갔는지 기록이 없어 장비를 지우지 않는다.");
            }

            // A generated loadout: the profile sent no property into the raid, so there is nothing to lose.
            if (gearKind == SpawnGearKind.Temporary)
            {
                return new OutcomePlan(OutcomeAction.Nothing, "임시 장비로 나가 프로필의 재산을 잃지 않는다.");
            }

            if (escaped)
            {
                return new OutcomePlan(OutcomeAction.Nothing, "탈출에 성공해 장비를 지우지 않는다.");
            }

            // The only line that deletes anything is reached by an explicit match, not by elimination:
            // a SpawnGearKind member added later, or an undefined value cast into the enum, must not strip.
            if (gearKind == SpawnGearKind.Real && !escaped)
            {
                return new OutcomePlan(OutcomeAction.StripWornGear, "실물 장비로 나갔다가 돌아오지 못해 착용 장비를 지운다.");
            }

            // Fail closed: a gear kind this code does not know never deletes equipment.
            return new OutcomePlan(OutcomeAction.Nothing, "알 수 없는 장비 종류라 장비를 지우지 않는다.");
        }
    }
}
