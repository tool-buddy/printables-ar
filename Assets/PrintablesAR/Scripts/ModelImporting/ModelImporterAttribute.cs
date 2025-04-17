using System;
using JetBrains.Annotations;

namespace ToolBuddy.PrintablesAR.ModelImporting
{
    [AttributeUsage(
        AttributeTargets.Class,
        Inherited = false
    )]
    public class ModelImporterAttribute : Attribute
    {
        /// <summary>
        /// The file extension that this model loader can handle.
        /// Is in lower case, without the leading dot.
        /// Example: "obj"
        /// </summary>
        [NotNull]
        public string FileExtension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelImporterAttribute"/> class.
        /// </summary>
        /// <param name="fileExtension"> The file extension that this model loader can handle.
        /// Should be in lower case, without the leading dot.
        /// Example: "obj" 
        /// </param>
        public ModelImporterAttribute(
            [NotNull] string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentException(
                    "File extension cannot be null or empty.",
                    nameof(fileExtension)
                );
            if (fileExtension.ToLowerInvariant() != fileExtension)
                throw new ArgumentException(
                    "File extension must be in lower case.",
                    nameof(fileExtension)
                );
            if (fileExtension.StartsWith("."))
                throw new ArgumentException(
                    "File extension must not start with a dot.",
                    nameof(fileExtension)
                );

            FileExtension = fileExtension.ToLowerInvariant();
        }
    }
}