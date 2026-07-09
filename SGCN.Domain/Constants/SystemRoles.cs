namespace SGCN.Domain.Constants;

public static class SystemRoles
{
    public const string Administrateur = "Administrateur";
    public const string AgentEtatCivil = "AgentEtatCivil";
    public const string Medecin = "Medecin";
    public const string Citoyen = "Citoyen";

    public static readonly IReadOnlyCollection<string> All =
    [
        Administrateur,
        AgentEtatCivil,
        Medecin,
        Citoyen
    ];
}
