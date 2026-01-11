using LabExtended.Extensions;

using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LabExtended.Core
{
	/// <summary>
	/// Used to print messages to the server console.
	/// </summary>
    public static class ApiLog
    {
#if DEBUG
		private static bool debugOverride = true;
#else
		private static bool debugOverride = false;
#endif

		/// <summary>
		/// Gets a value indicating whether debug mode is currently enabled for the application.
		/// </summary>
		/// <remarks>Debug mode may be enabled by configuration or programmatically. When enabled, additional
		/// diagnostic information may be logged or displayed to assist with troubleshooting.</remarks>
		public static bool IsDebugEnabled => debugOverride || ApiLoader.BaseConfig == null || ApiLoader.BaseConfig.DebugEnabled;

		/// <summary>
		/// Whether or not True Color formatting is enabled.
		/// </summary>
		public static bool IsTrueColorEnabled => ApiLoader.BaseConfig == null || ApiLoader.BaseConfig.TrueColorEnabled;

		/// <summary>
		/// Whether or not to prepend assembly names when extracting log source type.
		/// </summary>
		public static bool PrependAssemblyName { get; set; } = true;
        
	    /// <summary>
	    /// Prints an INFO message to the console.
	    /// </summary>
	    /// <param name="msg">The message.</param>
        public static void Info(object msg) 
	        => Info(null, msg);
        
	    /// <summary>
	    /// Prints a WARN message to the console.
	    /// </summary>
	    /// <param name="msg">The message.</param>
        public static void Warn(object msg) 
	        => Warn(null, msg);
        
	    /// <summary>
	    /// Prints an ERROR message to the console.
	    /// </summary>
	    /// <param name="msg">The message.</param>
        public static void Error(object msg) 
	        => Error(null, msg);
        
	    /// <summary>
	    /// Prints a DEBUG message to the console.
	    /// </summary>
	    /// <param name="msg">The message.</param>
        public static void Debug(object msg) 
	        => Debug(null, msg);

	    /// <summary>
	    /// Prints an INFO message to the console.
	    /// </summary>
	    /// <param name="source">Source of this message.</param>
	    /// <param name="msg">The message.</param>
	    /// <exception cref="ArgumentNullException"></exception>
        public static void Info(string? source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            AppendLog($"&7[&b&6INFO&B&7] &7[&b&2{source}&B&7]&r {msg}", ConsoleColor.White);
        }

	    /// <summary>
	    /// Prints a WARN message to the console.
	    /// </summary>
	    /// <param name="source">Source of this message.</param>
	    /// <param name="msg">The message.</param>
	    /// <exception cref="ArgumentNullException"></exception>
        public static void Warn(string? source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
	        if (source is null || source.Length < 1 || source[0] == ' ')
		        source = GetSourceType();

	        AppendLog($"&7[&b&3WARN&B&7] &7[&b&3{source}&B&7]&r {msg}", ConsoleColor.White);
        }

	    /// <summary>
	    /// Prints an ERROR message to the console.
	    /// </summary>
	    /// <param name="source">Source of this message.</param>
	    /// <param name="msg">The message.</param>
	    /// <exception cref="ArgumentNullException"></exception>
        public static void Error(string? source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
	        if (source is null || source.Length < 1 || source[0] == ' ')
		        source = GetSourceType();

	        AppendLog($"&7[&b&1ERROR&B&7] &7[&b&1{source}&B&7]&r {msg}", ConsoleColor.White);
        }

	    /// <summary>
	    /// Prints a DEBUG message to the console.
	    /// </summary>
	    /// <param name="source">Source of this message.</param>
	    /// <param name="msg">The message.</param>
	    /// <exception cref="ArgumentNullException"></exception>
        public static void Debug(string? source, object msg)
        {
            if (msg is null)
                throw new ArgumentNullException(nameof(msg));

            if (!IsDebugEnabled)
                return;

            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            AppendLog($"&7[&b&5DEBUG&B&7] &7[&b&5{source}&B&7]&r {msg}", ConsoleColor.White);
        }

		/// <summary>
		/// Retrieves the name of the source type from the current call stack, optionally including the assembly name if
		/// configured.
		/// </summary>
		/// <remarks>This method inspects the call stack to determine the most relevant source type, excluding
		/// compiler-generated types and internal logging infrastructure. If the source type is compiler-generated, its
		/// declaring type or sanitized name is used. The returned value may vary depending on the call context and
		/// configuration.</remarks>
		/// <returns>A string representing the source type name. If assembly name prepending is enabled and available, the result is
		/// formatted as "AssemblyName / TypeName"; otherwise, only the type name is returned.</returns>
        public static string GetSourceType()
        {
            var trace = new StackTrace();
            var frames = trace.GetFrames();
			var name = "LabExtended";
			var assembly = default(Assembly);

            for (var i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                var method = frame.GetMethod();

                if (method is null)
                    continue;

                if (method.DeclaringType is null)
                    continue;

                if (method.DeclaringType == typeof(ApiLog))
                    continue;

                var type = method.DeclaringType;

				if (type.HasAttribute<CompilerGeneratedAttribute>())
				{
					if (type.DeclaringType != null)
					{
						name = type.DeclaringType.Name;
						assembly = type.DeclaringType.Assembly;

						break;
					}
					else
					{
						name = type.Name.SanitizeCompilerGeneratedName();
						assembly = type.Assembly;

						break;
					}
				}
				else
				{
					name = type.Name;
					assembly = type.Assembly;

					break;
				}
            }

			if (PrependAssemblyName && assembly != null)
			{
				var asmName = assembly.GetName();

				if (!string.IsNullOrEmpty(asmName?.Name))
					return string.Concat(asmName!.Name, " / ", name);

				return string.Concat(assembly.FullName, " / ", name);
			}

			return name;
        }

        private static void AppendLog(string msg, ConsoleColor color)
        {
			if (IsTrueColorEnabled)
				msg = msg.FormatTrueColorString("7", false, false);
			else
				msg = msg.SanitizeTrueColorString();

			ServerConsole.AddLog(msg, color);
        }
    }
}