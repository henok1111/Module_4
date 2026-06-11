// Models/Assessment.cs
// ─────────────────────────────────────────────────────────────
// This is the data shape returned by the assessments endpoint
// We use a record because assessment results are facts
// Once a grade is recorded it should not be mutated
// ─────────────────────────────────────────────────────────────

public record AssessmentResult(
    string CourseCode,   // e.g. "CS-101"
    string StudentId,    // e.g. "S-001"
    string LetterGrade   // e.g. "A"
);