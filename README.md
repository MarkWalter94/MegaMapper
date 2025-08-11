Everyone in their life at a certain point needed a mapping library.
Every mapping library is a mess to deal with, this is also a mess, but less.

This library was born to meet our needs to have asyncronus DI compatible mappings, for example suppose this:

Suppose to have a table in your db that maps a pair of keys, for example

| keyA | keyB |
|------|------|
| 100  | aaa  |
| 200  | bbb  |

And you want to map and ObjectA to an ObjectB and viceversa. You need a lookup, that would do an asyncronus operation.
With MegaMapper you can do this, and it's simple!



