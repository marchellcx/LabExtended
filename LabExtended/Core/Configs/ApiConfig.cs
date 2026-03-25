using LabExtended.Core.Configs.Sections;

using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    /// <summary>
    /// Contains API-related configuration.
    /// </summary>
    public class ApiConfig
    {
        /// <summary>
        /// Gets or sets the probability of SCP-956 utilizing the Capybara model.
        /// </summary>
        [Description("Sets the chance of SCP-559 using the Capybara model.")]
        public float? Scp956CapybaraChance { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether SCP-2536 spawning should be disabled.
        /// </summary>
        [Description("Whether or not the SCP-2536 should be prevented from spawning.")]
        public bool Scp2536Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the host player (server console) has permission to execute all
        /// commands.
        /// </summary>
        [Description("Whether or not the host player (server console) should be able to execute all commands.")]
        public bool HostPermissions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether regular expression patterns are used to match permissions.
        /// </summary>
        [Description("Whether or not to use a regex pattern to match permissions.")]
        public bool RegexPermissions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether permission requirements are ignored.
        /// </summary>
        [Description("Whether or not to ignore permission requirements..")]
        public bool OverridePermissions { get; set; } = false;

        /// <summary>
        /// Gets or sets a list of IPs that should be overriden by the player's token request IP.'
        /// </summary>
        [Description("Sets a list of IPs that should be overriden by the player's token request IP.")]
        public List<string> TokenIpOverride { get; set; } = new()
        {

        };

        /// <summary>
        /// Gets or sets the configuration settings for file storage.
        /// </summary>
        [Description("File Storage configuration.")]
        public StorageSection StorageSection { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the configuration settings for voice chat.
        /// </summary>
        [Description("Voice chat configuration.")]
        public VoiceSection VoiceSection { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the configuration settings for the command system.
        /// </summary>
        [Description("Command System configuration.")]
        public CommandSection CommandSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the configuration settings for the hint system.
        /// </summary>
        [Description("Hint system configuration.")]
        public HintSection HintSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the patching options to be applied.
        /// </summary>
        [Description("Patching options.")] 
        public PatchSection PatchSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the Unity Engine player loop configuration.
        /// </summary>
        [Description("Unity Engine Player Loop configuration.")]
        public LoopSection LoopSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the configuration settings for additional application features.
        /// </summary>
        [Description("Configuration for other things.")]
        public OtherSection OtherSection { get; set; } = new();
    }
}