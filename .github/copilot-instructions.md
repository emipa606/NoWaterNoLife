# Copilot Instructions for No Water, No Life (Continued)

## Mod Overview and Purpose
**No Water, No Life (Continued)** is a mod initially created by himesama and schooss, further updated by grapenuts, and now continued by the community. The mod introduces a new "Need Water" mechanic to RimWorld pawns, significantly altering gameplay by requiring pawns to manage their hydration alongside other survival needs. It includes a variety of new gameplay elements focused on water acquisition, transport, and purification.

## Key Features and Systems
- **Water Needs for Pawns:** Adds a hydration need to pawns, similar to food and rest.
- **Water Systems:** Implements wells, pumps, water pipes, faucets, water filters, and other constructions for water collection and management.
- **Integration with Other Mods:** Patches for VGP Core, VGP Gourmet, and Alpha Animals.
- **Industrial Water Filters:** New industrial water filters with ten times the throughput capacity.
- **Translation Support:** New Polish translation added (thanks to BLyy).

## Coding Patterns and Conventions
- **Class Conventions:** All classes are designed as either `internal` or `public` depending on the scope. Files usually follow the naming pattern `Functionality_Description`, e.g., `Building_Well.cs`.
- **Inheritances:** Key classes inherit from `Building_WorkTable`, `Building_WaterNet`, and other abstract or base classes to implement behavior specific to water management.
- **Internal Methods:** Keeping helper methods like `CanDrinkFor`, `DrawWater`, private or protected unless needed publicly.
  
## XML Integration
- **DefModExtensions:** Utilize `DefModExtension` for extending XML definitions and adding custom attributes where necessary.
- **Translation Files:** Ensure translations are stored in appropriate `Languages` folders with flexibility for future localization updates.
- **XML Patch Files:** Use XML patch operations for integrating content from other mods.

## Harmony Patching
Harmony is used to patch the game functions, particularly those involving water needs and inter-mod compatibility:
- Implement patches cautiously, ensuring compatibility with the base game and other mods.
- Example of a patch file could be:
  csharp
  [HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
  public static class MyPatch {
      static bool Prefix(ref ReturnType __result) {
          // custom logic
          return false; // to skip original method
      }
  }
  

## Suggestions for Copilot
- **Water Mechanic Enhancements:** Recommend new features such as weather effects impacting water needs (e.g. rain increases water availability).
- **Performance Optimization:** Leverage Copilot to identify parts of the code that might be optimized for performance, especially around water calculations.
- **Code Safety Checks:** Add null checking and exception handling where necessary, Copilot can assist in identifying such areas.
- **Feature Integrations:** Suggest expansions like compatibility patches for popular mods, using suggestions from Copilot to generate templates.
- **User Experience Improvements:** Prompt Copilot to suggest code for HUD enhancements showing water status or alerts about dehydration risks.

By incorporating these elements, the `No Water, No Life (Continued)` mod aims to offer an engaging and challenging survival mechanic that feels seamlessly integrated into the RimWorld experience.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
