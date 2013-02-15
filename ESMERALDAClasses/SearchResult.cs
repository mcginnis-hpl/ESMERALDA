﻿namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;

    public class SearchResult
    {
        public string DatasetBriefDescription = string.Empty;
        public string DatasetDescription = string.Empty;
        public Guid DatasetID = Guid.Empty;
        public string DatasetName = string.Empty;
        public string DisplayFields = string.Empty;
        public bool geo_match = false;
        public string latlon_database_name = string.Empty;
        public List<Guid> latlon_metric_id = new List<Guid>();
        public List<string> latlon_sql_column_name = new List<string>();
        public string latlon_sql_table_name = string.Empty;
        public List<string> MatchingFields = new List<string>();
        public List<string> MatchingKeywords = new List<string>();
        public Guid ProjectID = Guid.Empty;
        public string ProjectName = string.Empty;
        public int Score = 0;
        public string URL = string.Empty;
        public bool isExternal = false;
        public string SourceName = string.Empty;
        public double min_lat = double.NaN;
        public double min_lon = double.NaN;
        public double max_lat = double.NaN;
        public double max_lon = double.NaN;
    }
}

