using System;

namespace CaveGame.Common;

public class MissingGameDataException : ApplicationException { 
    public string MissingFilename { get; set; }
    public string MissingFilepath { get; set; }
    public string FilePurpose { get; set; }
    public string WhatToDo { get; set; }


}

public class MissingContentFolderException : MissingGameDataException { }
public class MissingScriptException : MissingGameDataException { }
public class MissingSoundEffectException : MissingGameDataException { }
public class MissingTextureException : MissingGameDataException { }