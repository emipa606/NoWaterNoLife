# GitHub Copilot Instructions for No Water, No Life (Continued)

## Mod Overview and Purpose

The "No Water, No Life (Continued)" mod enhances the immersive experience of RimWorld by introducing a "Need Water" mechanic for pawns. Originally initiated by himesama and schooss, this mod updates the previous version based on grapenuts' 1.1 release. The mod introduces various components to manage water resources effectively, including wells, pumps, water pipes, faucets, water cleaners, and much more.

**Important Note:** This mod should not be applied mid-game to save files. It is recommended to start a new game with this mod. Removal from an existing save game is also not advised.

## Key Features and Systems

- **Water Management**: Introduces a comprehensive system to manage water needs for both pawns and colonies.
- **Infrastructure**: Includes buildings and tools such as water wells, pumps, and pipelines to facilitate water gathering and distribution.
- **Compatibility Patches**: Updates include patches for Alpha Animals and VGP_Core & VGP_Gourmet.
- **Translation**: Added Polish translation along with updates from other contributors for multilingual support.
- **Industrial Water Filters**: Provides buildings with increased capacity for processing water.
- **Sound Enhancements**: Updated custom sounds instead of using built-in sounds.

## Coding Patterns and Conventions

- **Naming Conventions**: Classes and methods follow PascalCase naming convention, while variables within methods are typically camelCase.
- **Access Modifiers**: Prefer 'internal' for classes that don't need external access, ensuring encapsulation.
- **Inheritance**: Extensively uses base classes and interfaces (e.g., Building_WorkTable, IBuilding_DrinkWater) for functionality sharing and polymorphism.
- **Methods**: Keep methods focused and concise, with descriptive names indicating their purpose.

## XML Integration

- **Def Mod Extensions**: Classes such as `DefExtension_NeedWater` and `DefExtension_ThirstRate` allow XML configuration for expanding the mod's functionality and adjusting to different scenarios.
- **Recipe Definitions**: Managed using classes like `GetWaterRecipeDef` to define and extend water-related recipes.

## Harmony Patching

- The mod begins modifying existing core behavior using Harmony patches. Files include changes to dialogues and caravan systems to integrate the water need.
- Prefer structuring patches to minimize conflicts with other mods and ensure consistent behavior.

## Suggestions for Copilot

1. **Water Net Code Enhancements**: Improve water flow calculations in `CompWaterNet.cs` by optimizing how inputs and outputs are handled.
2. **Dialogue Interface Adjustments**: Enhance user interfaces in `Dialog_FormCaravan_DoWindowContents.cs` to better inform players about water resources.
3. **Additional Sound Integration**: Write custom water-related events in classes that involve interaction, e.g., `JobDriver_DrinkWater`.
4. **Performance Improvements**: Use tools like `CompWaterNet` to optimize calculations and interactions within the water management network.
5. **Debug Tools**: Develop debugging tools within `GSForDebug.cs` to assist in mod development and testing.

---

By following these guidelines, contributors can ensure consistent code quality and maintainability while utilizing GitHub Copilot's capabilities efficiently.


This markdown file provides detailed instructions for contributors using GitHub Copilot on the "No Water, No Life (Continued)" mod for RimWorld, outlining the purpose, features, coding practices, and integration aspects crucial for development.
