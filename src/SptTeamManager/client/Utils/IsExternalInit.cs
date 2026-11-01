// net472 has no System.Runtime.CompilerServices.IsExternalInit; the linked shared records
// (shared/ProfileRosterPolicy.cs) need it for init accessors. Compiler-only marker type.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
