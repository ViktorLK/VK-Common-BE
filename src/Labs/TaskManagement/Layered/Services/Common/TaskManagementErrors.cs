using VK.Blocks.Core.Results;

namespace VK.Labs.TaskManagement.Layered.Services.Common;

public static class TaskManagementErrors
{
    public static class Auth
    {
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized);
    }

    public static class Projects
    {
        public static readonly Error NotFound = new("Projects.NotFound", "The project was not found.", ErrorType.NotFound);
        public static readonly Error Forbidden = new("Projects.Forbidden", "You do not have access to this project.", ErrorType.Forbidden);
    }

    public static class Tasks
    {
        public static readonly Error NotFound = new("Tasks.NotFound", "The task was not found.", ErrorType.NotFound);
    }
}
