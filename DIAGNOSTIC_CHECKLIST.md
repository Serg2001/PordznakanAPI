# TeacherSubject Save Diagnostic Checklist

## Current Status
❌ TeacherSubject records are NOT being saved to database

## What to Check in Your Logs

When you run the sync, look for these log messages in order:

### 1. Sample Teacher Structure
```
📋 Sample teacher X fields: person_id, school_id, subjects, ...
```
**Action:** Verify "subjects" appears in the field list

### 2. Subjects Field Detection
```
📚 Sample teacher X 'subjects' field exists. Type: Array, Value: ...
```
**OR**
```
⚠️ Sample teacher X does NOT have 'subjects' field!
```
**Action:** If you see the warning, the JSON doesn't have subjects

### 3. Subject Processing
```
📚 Processing X subjects for teacher Y
```
**Action:** This confirms subjects are being found and processed

### 4. Subject Addition
```
✅ Adding subject to context: TeacherId=..., SubjectId=..., ...
```
**Action:** This confirms subjects are being added to DbContext

### 5. Summary
```
📊 Subject processing summary: TotalAdded=X, TotalSkipped=Y, ...
```
**Action:** Check if TotalAdded > 0

### 6. Save Attempt
```
💾 SAVING subjects to database now...
✅ SaveChangesAsync completed. Entities saved: X
```
**Action:** Check if entities saved > 0

### 7. Verification
```
✅ Verification: Found X subjects in database
```
**Action:** This confirms subjects were actually saved

## Common Issues & Solutions

### Issue 1: "No 'subjects' field found"
**Solution:** The JSON doesn't have a subjects array. Check your API response format.

### Issue 2: "TotalAdded: 0"
**Solution:** Subjects exist but all are being skipped. Check for:
- Invalid subject_id (must be integer)
- Missing required fields (grade, classifier, subject_title)

### Issue 3: "Entities saved: 0"
**Solution:** SaveChangesAsync returned 0. Check for database errors or constraints.

### Issue 4: "Found 0 subjects in database"
**Solution:** Save failed silently. Check database logs and exceptions.

## Next Steps

1. **Run the sync** and copy ALL log messages related to subjects
2. **Share the logs** - especially:
   - Sample teacher fields
   - Subject processing messages
   - Any error messages
3. **Check database** - Verify TeacherSubjects table exists and has correct schema

## Database Migration Check

If the database schema doesn't match the model, run:
```bash
dotnet ef migrations add RemoveTeacherIdFromTeacherSubject
dotnet ef database update
```

This will ensure the database matches your model structure (KtakTeacherId instead of TeacherId).

