# QL4BIM
Advanced query language for building information models 

Solution Setup: 
Create a pure x32 and/or x64 environment for all projects. Otherwise the IfcEngine DLLs and schemas will not be found at run time.

A query example:

entities = ImportModel("path\2\ifcfile.ifc")

walls = TypeFilter (entities is IfcWall)

windows = TypeFilter (entities is IfcWindow)

a[wall|window] = Touches(walls, windows)
