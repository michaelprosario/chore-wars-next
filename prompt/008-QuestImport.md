# Quest Import Feature - Implementation Plan

## Overview
Implement functionality for Dungeon Masters to import multiple quests from a JSON file, streamlining quest creation for their party.

## Requirements Summary
- **User Role**: Dungeon Master (DM) only
- **Input**: JSON file containing quest data
- **Required Fields**: Title, Description, XPReward, GoldReward, DifficultyLevel
- **Auto-generated**: Quest Id (new Guid), CreatedAt (DateTime.UtcNow), CreatedByDMId, PartyId
- **Defaults**: IsActive (true), QuestType (OneTime), Stat bonuses (0), Navigation properties
- **Output**: Bulk import of quests with validation and error reporting

## JSON Schema

### Import File Format
```json
{
  "quests": [
    {
      "title": "Quest Title (required, max 200 chars)",
      "description": "Quest description (required)",
      "xpReward": 50,
      "goldReward": 10,
      "difficulty": "Easy | Medium | Hard (required)",
      "questType": "OneTime | Daily | Weekly (optional, default: OneTime)",
      "strengthBonus": 0,
      "intelligenceBonus": 0,
      "constitutionBonus": 0
    }
  ]
}
```

### Example Import File
```json
{
  "quests": [
    {
      "title": "Wash the Dishes",
      "description": "Clean all dishes in the sink and put them away",
      "xpReward": 50,
      "goldReward": 10,
      "difficulty": "Easy",
      "questType": "Daily",
      "constitutionBonus": 1
    },
    {
      "title": "Vacuum the House",
      "description": "Vacuum all carpeted areas in the house",
      "xpReward": 75,
      "goldReward": 15,
      "difficulty": "Medium",
      "questType": "Weekly",
      "strengthBonus": 1
    },
    {
      "title": "Organize the Garage",
      "description": "Sort, organize, and clean the entire garage",
      "xpReward": 200,
      "goldReward": 50,
      "difficulty": "Hard",
      "questType": "OneTime",
      "strengthBonus": 2,
      "constitutionBonus": 1
    }
  ]
}
```

## Validation Rules

### Required Fields
- **title**: 1-200 characters
- **description**: 1-1000 characters (non-empty)
- **xpReward**: Integer, 1-1000
- **goldReward**: Integer, 0-1000
- **difficulty**: Must be "Easy", "Medium", or "Hard" (case-insensitive)

### Optional Fields
- **questType**: "OneTime", "Daily", or "Weekly" (default: "OneTime", case-insensitive)
- **strengthBonus**: Integer, 0-10 (default: 0)
- **intelligenceBonus**: Integer, 0-10 (default: 0)
- **constitutionBonus**: Integer, 0-10 (default: 0)

### Auto-Assigned Fields
- **Id**: New Guid generated per quest
- **PartyId**: DM's current party
- **CreatedByDMId**: Current DM's user ID
- **CreatedAt**: DateTime.UtcNow
- **IsActive**: true

### Validation Error Handling
- Return detailed errors for each invalid quest
- Include quest index/title in error messages
- Allow partial import (import valid quests, report invalid ones) OR all-or-nothing import
- Validate JSON structure before processing individual quests

## Implementation Architecture

### 1. Core Layer Changes

#### New DTOs (`ChoreWars.Core/Commands/QuestCommands.cs`)
```csharp
public class ImportQuestsCommand
{
    public required List<QuestImportDto> Quests { get; set; }
    public required Guid PartyId { get; set; }
    public required Guid DMId { get; set; }
}

public class QuestImportDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int XPReward { get; set; }
    public required int GoldReward { get; set; }
    public required string Difficulty { get; set; }
    public string? QuestType { get; set; }
    public int StrengthBonus { get; set; } = 0;
    public int IntelligenceBonus { get; set; } = 0;
    public int ConstitutionBonus { get; set; } = 0;
}
```

#### Service Interface Update (`ChoreWars.Core/Interfaces/IQuestService.cs`)
```csharp
Task<AppResult<List<QuestDto>>> ImportQuestsAsync(ImportQuestsCommand command);
```

#### Service Implementation (`ChoreWars.Core/Services/QuestService.cs`)
- Validate each quest in the import list
- Convert QuestImportDto to Quest entity
- Parse and validate enum values (DifficultyLevel, QuestType)
- Assign auto-generated fields (Id, PartyId, CreatedByDMId, CreatedAt, IsActive)
- Bulk insert using repository
- Return AppResult with success/failure details

### 2. Web Layer Changes

#### Controller Action (`ChoreWars.Web/Controllers/DMController.cs`)
```csharp
[HttpGet]
[Authorize(Roles = "DungeonMaster")]
public IActionResult Import()
{
    return View();
}

[HttpPost]
[Authorize(Roles = "DungeonMaster")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Import(IFormFile jsonFile)
{
    // Validate file upload
    // Read JSON content
    // Deserialize to ImportQuestsCommand
    // Call IQuestService.ImportQuestsAsync
    // Display results/errors
}
```

#### View (`ChoreWars.Web/Views/DM/Import.cshtml`)
- File upload form (accept .json files)
- JSON schema documentation
- Example file download link (or embedded example)
- Success/error message display
- List of successfully imported quests
- List of validation errors with quest details

#### Navigation Update
- Add "Import Quests" link to DM Index page (`Views/DM/Index.cshtml`)

### 3. Repository Layer
No changes needed - existing `QuestRepository.AddAsync()` and bulk insert via `AddRangeAsync()` can be used.

### 4. Database
No migrations needed - existing Quest table schema supports all fields.

## Implementation Steps

### Phase 1: Core Logic
1. Create `QuestImportDto` and `ImportQuestsCommand` in `/src/ChoreWars.Core/Commands/QuestCommands.cs`
2. Add `ImportQuestsAsync` method signature to `IQuestService`
3. Implement validation logic in `QuestService`:
   - Validate required fields
   - Validate value ranges
   - Parse and validate enums
   - Check DM owns the party
4. Implement quest entity creation and bulk insert
5. Return `AppResult<List<QuestDto>>` with imported quests or errors

### Phase 2: Web Interface
1. Create `Import.cshtml` view with:
   - File upload form
   - JSON schema documentation
   - Example JSON
2. Implement `Import()` GET action in `DMController`
3. Implement `Import(IFormFile)` POST action in `DMController`:
   - File validation (max size, .json extension)
   - JSON deserialization with error handling
   - Service call and result processing
4. Add navigation link to Import page from DM Index

### Phase 3: Testing & Documentation
1. Unit tests for `QuestService.ImportQuestsAsync`:
   - Valid import
   - Invalid field values
   - Missing required fields
   - Invalid enum values
   - Malformed JSON
2. Integration tests for controller
3. Update user documentation with import feature guide

## Error Handling Strategy

### File Upload Errors
- **No file selected**: "Please select a JSON file to import"
- **Invalid file type**: "Only .json files are accepted"
- **File too large**: "File size must be less than 5MB"
- **Cannot read file**: "Unable to read file contents"

### JSON Parsing Errors
- **Malformed JSON**: "Invalid JSON format. Please check your file syntax."
- **Missing 'quests' array**: "JSON must contain a 'quests' array"
- **Empty quests array**: "No quests found in file"

### Quest Validation Errors
Return per-quest errors with format:
```
Quest #1 "Wash Dishes":
  - XPReward must be between 1 and 1000
  - Difficulty 'EXTREME' is invalid. Valid values: Easy, Medium, Hard
```

### Authorization Errors
- **Not a DM**: Redirect to unauthorized page
- **No party assigned**: "You must be assigned to a party to import quests"

## Import Strategy Decision

**Recommendation**: **All-or-Nothing Import**
- Validate all quests first
- Only insert if ALL quests are valid
- Prevents partial/inconsistent data
- Simpler rollback (no database changes if any validation fails)

**Alternative**: Partial Import
- Import valid quests, skip invalid ones
- Return list of successful imports + list of failed quests
- More complex but allows salvaging good data from files with some errors

**Decision Point**: Ask user preference or default to all-or-nothing for data integrity.

## UI/UX Considerations

### Import Page Elements
1. **Header**: "Import Quests from JSON"
2. **Instructions Section**:
   - Brief description of feature
   - Link to example file
   - JSON schema documentation
3. **File Upload Form**:
   - File input (accept=".json")
   - Submit button
   - Cancel/Back to Quests button
4. **Results Section** (after import):
   - Success message with count
   - Table of imported quests
   - Error details if any

### Schema Documentation Display
Use collapsible sections or tabs:
- **Quick Start**: Link to download example file
- **Schema Reference**: Field descriptions and validation rules
- **Examples**: 2-3 example quest objects

## Files to Create/Modify

### New Files
- `/src/ChoreWars.Web/Views/DM/Import.cshtml`

### Modified Files
- `/src/ChoreWars.Core/Commands/QuestCommands.cs` (add DTOs)
- `/src/ChoreWars.Core/Interfaces/IQuestService.cs` (add method)
- `/src/ChoreWars.Core/Services/QuestService.cs` (implement import)
- `/src/ChoreWars.Web/Controllers/DMController.cs` (add Import actions)
- `/src/ChoreWars.Web/Views/DM/Index.cshtml` (add navigation link)

## Security Considerations

1. **Authorization**: Enforce `[Authorize(Roles = "DungeonMaster")]`
2. **Party Validation**: Ensure DM owns the target party
3. **File Size Limit**: Max 5MB to prevent DoS
4. **JSON Bomb Protection**: Limit array size (e.g., max 100 quests per import)
5. **Antiforgery Token**: Use `[ValidateAntiForgeryToken]` on POST
6. **Input Sanitization**: Validate/escape Title and Description for XSS prevention

## Future Enhancements (Out of Scope)

- Export quests to JSON format
- Import from CSV
- Template library of common quests
- Duplicate detection during import
- Quest categories/tags
- Bulk edit imported quests before final save

## Success Criteria

- ✅ DM can upload a valid JSON file with multiple quests
- ✅ All required fields are validated before import
- ✅ Invalid quests are rejected with clear error messages
- ✅ Successfully imported quests appear in DM's quest list
- ✅ Auto-generated fields (Id, CreatedAt, etc.) are correctly assigned
- ✅ JSON schema is clearly documented on import page
- ✅ Import page is accessible from DM Index page
- ✅ Only DMs can access the import feature
- ✅ Large files and malformed JSON are handled gracefully
