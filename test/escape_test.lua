local a = "\t\n\r"
print(a)

local b = "\65"
print(b)

b = "\x41"
print(b)

b = "\"hello world\""
print(b)

-- comment
local c = "\u{5c0f}"
print(c)

c = "\xab"
print(c)

c = "\147"
print(c)

--[==[

multiline comment

]==]--

local page = [[
<HTML>
<HEAD>
<TITLE>An HTML Page</TITLE>
</HEAD>
<BODY>
 <A HREF="http://www.lua.org">Lua</A>
</BODY>
</HTML>
]]
    
print(page)

a = 10
print(a)

a = 10 * 2
print(a)

a = 0x16
print(a)

a = 10e-2
print(a)

a = 0x16p-3
print(a)