using System.Globalization;

namespace organizador_dietas_santo_tomas.Models;

// Presentation-only contracts. No EF entities or persistence dependencies.
public record AvatarUiModel(string Name, bool Large = false)
{
    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Take(2).Select(part => StringInfo.GetNextTextElement(part))).ToUpperInvariant() is { Length: > 0 } initials
            ? initials : "?";
}

public record ProfileUiModel(string Name, string Email, string Role, string? Detail = null);
public record StatUiModel(string Label, string Value, string Description, string Icon = "chart");
public record AlertUiModel(string Title, string? Message = null, string Tone = "success");
public record BadgeUiModel(string Text, string Tone = "neutral");
public record EmptyUiModel(string Title, string Description, string Icon = "leaf");
public record DietUiModel(string Name, string Description, int Calories, int Carbohydrates,
    int Protein, int Fat, string Status = "Borrador");
public record MealUiModel(string Name, string Time, string Food, string Portion, int Calories);
public record AssignmentUiModel(string PatientName, string DietName, DateOnly Start, DateOnly End,
    string Status = "Pendiente");
public record SearchUiModel(string Id, string Label, string Placeholder);
