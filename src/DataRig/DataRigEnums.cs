public enum EvidenceTarget
{
    DisplayName,
    UserHandle,
    DatePosted,
    ProfilePicture,
    Title,
    BodyText,
    StoryImage
}

public enum EvidenceSource
{
    DataRig,
    Photograph,
    Dialogue,
    WorldInteraction
}

public enum EvidenceCategory
{
    BotSign,
    Impersonation,
    SuspiciousClaim,
    VisualAnomaly,
    Contradiction,
    Other
}

public enum PlayerVerdict
{
    Undecided,
    Real,
    Fake
}
