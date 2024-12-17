namespace UKSF.Launcher.Patching {
    internal class RepoAction {
        public readonly Addon Addon;
        public readonly string AddonFile;

        private RepoAction(Addon addon, string addonFile) {
            Addon = addon;
            AddonFile = addonFile;
        }


        public class AddedAction : RepoAction {
            public AddedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }

        public class DeletedAction : RepoAction {
            public DeletedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }

        public class ModifiedAction : RepoAction {
            public ModifiedAction(Addon addon, string addonFile) : base(addon, addonFile) { }
        }
    }
}