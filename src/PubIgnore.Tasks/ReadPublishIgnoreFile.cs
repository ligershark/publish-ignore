namespace PubIgnore.Tasks {    
    using System;
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
        //public ITaskItem[] LinesFromFile { get; set; }
        public string[] LinesFromFile { get; set; }     

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

            // System.Collections.Generic.List<ITaskItem> linesNotComments = new System.Collections.Generic.List<ITaskItem>();
            System.Collections.Generic.List<string> linesNotComments = new System.Collections.Generic.List<string>();
            foreach(string line in allLinesRaw){
                if (string.IsNullOrEmpty(line))
                    continue;

                // trim the line and see if it starts with #
                string pattern = line;

                if (pattern.StartsWith("#"))
                    continue;

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

                    // for file patterns that start with \ or / we should 
                    //  remove the leading slash because MSBuild assumes that
                    //  don't match \\servername\file.name
                    string fileThatStartsWithASlashPattern = @"^[\\,/]{1}[^\\,/].*\.[^/]*";
                    if (System.Text.RegularExpressions.Regex.IsMatch(pattern, fileThatStartsWithASlashPattern)) {
                        // \sample.txt or \folder\sub\somefile.txt
                        pattern = pattern.Substring(1);
                    }

                    // if its a directory we should append **\* to the end
                    if (pattern.EndsWith(@"/") || pattern.EndsWith(@"\"))
                    {
                        pattern = string.Format(@"{0}**\*", pattern);
                    }
                }
                #endregion
                
                // add it to the list to be returned
                // linesNotComments.Add(new TaskItem(pattern));
                linesNotComments.Add(pattern);
            }

            // doesn't work from an inline task for some reason
            // this.LinesFromFile = linesNotComments.ToArray();

            // this.LinesFromFile = new ITaskItem[linesNotComments.Count];
            this.LinesFromFile = new string [linesNotComments.Count];
            for (int i = 0; i < linesNotComments.Count; i++) {
                this.LinesFromFile[i] = linesNotComments[i].ToString();
            }

            Log.LogMessage("Finished reading publish.ignore file at [{0}]. Found [{0}] lines which are not comments or blank.", this.FilePath, this.LinesFromFile.Length);

            return !Log.HasLoggedErrors;
        }
    }
}