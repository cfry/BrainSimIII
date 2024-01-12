# Glossary
Started Jan 12, 2024

**BrainSimulator**
A program that captures Common Sense Knowledge
and facilitates reasoning with that knowledge.
Written in C#

**thing**
The primary data structure for storing knowledge in BrainSimulator.
Has parts: label, relationships
A thing has a list of relationships, all of who's source
is the thing.

**relationship**
Describes the relationship between two things.
 has parts: source, type, target

**source**
A thing that is the first part of a relationship.

**type**
A thing that is the 2nd part of a relationship
that indicates the type of this relationship.
Examples: is_a, has_child, or any thing.

**inverse** 
A type that indicates the inverse relationship to 
another type, such as has_child and is_a.

**target**
A thing that is the 3rd part of a relationship.

**clause** 
A relationship between two relationships.

**label**
A string that is unique across BrainSimulator 
identifying a thing. Can be used as a reference to the thing
that has the label.

**confidence**
A property of a relationship that indicates how likely it is to be true.
A number between 0 and ???

**duration**
The amount of time that a relationship lives,
after which it will be deleted.
Units???

#Terms not defined on purpose
**relation**
