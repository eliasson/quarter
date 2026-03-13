namespace Quarter.HttpApi.Resources;

// ReSharper disable InconsistentNaming
public record ProjectAndActivitiesResourceOutput(IList<ProjectResourceOutput> projects, IList<ActivityResourceOutput> activities);
