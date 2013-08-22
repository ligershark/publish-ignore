namespace PubIgnore.Tasks {
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// This task will read the publish.ignore file and return the results
    /// </summary>
    public class ReadPublishIgnoreFile : Task {
        [Required]
        public string FilePath { get; set; }

        /// <summary>
        /// Contains the files which were read from the file, excluding
        /// any comments in the file
        /// </summary>
        [Output]
        public ITaskItem[] LinesFromFile { get; set; }        

        public override bool Execute() {
            System.Diagnostics.Debugger.Launch();
            // read the file line by line and exclude any lines which start with # or 
            //  just contain whitespace
            Log.LogMessage("Starting to read publish.ignore file at [{0}]", this.FilePath);

            if (!File.Exists(FilePath)) {
                string msg = string.Format("Unable to find the publish.ignore file at [{0}]",this.FilePath);
                Log.LogError(msg);
                return false;
            }

            // TODO: do this better
            string[] allLinesRaw = File.ReadAllLines(this.FilePath);

            List<ITaskItem> linesNotComments = new List<ITaskItem>();
            foreach(string line in allLinesRaw){
                if (string.IsNullOrEmpty(line))
                    continue;

                // trim the line and see if it starts with #
                string pattern = line;

                #region ConvertPatternIfNecessary - if this was defiend in an assembly this would be it's own method
                if (!string.IsNullOrEmpty(pattern)) {
                    pattern = pattern.Trim();
                    if (pattern.StartsWith("!"))
                    {
                        throw new NotSupportedException("The ! operator is not currently supported in publish.ignore");
                    }

                    // for patterns that match file and that do not start with \ or / we should prepend **\
                    if (!(pattern.EndsWith(@"\") || pattern.EndsWith(@"/"))) {
                        if (!(pattern.StartsWith(@"\") || pattern.StartsWith(@"/"))) {
                            pattern = string.Format(@"**\{0}",pattern);
                        }
                    }

                    // if its a directory we should append **\* to the end
                    if (pattern.EndsWith(@"/") || pattern.EndsWith(@"\"))
                    {
                        pattern = string.Format(@"{0}**\*", pattern);
                    }
                }
                #endregion


                if (pattern.StartsWith("#"))
                    continue;

                // add it to the list to be returned
                linesNotComments.Add(new TaskItem(pattern));
            }

            this.LinesFromFile = linesNotComments.ToArray();

            Log.LogMessage("Finished reading publish.ignore file at [{0}]. Found [{0}] lines which are not comments or blank.", this.FilePath, this.LinesFromFile.Length);

            return !Log.HasLoggedErrors;
        }
    }
}