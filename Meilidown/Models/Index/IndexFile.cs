﻿namespace Meilidown.Models.Index;

// ReSharper disable InconsistentNaming

public record IndexFile(string uid, string name, string content, int order, string location);