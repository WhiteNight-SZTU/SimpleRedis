# SimpleString
A tool for bulk adding String to Redis , based on WinUI3.  
向Redis中批量添加String的WinUI3程序。


However, i was trying to build a visual tool that no only allow user to bulk adding String, which also supported user to add List, HashList and more.  
But for me, as a beginner of WinUI3 and Redis, this is a big project and it will take too long, so perhaps I will update the relevant features in the future, or maybe not.  
我很想实现RedisInsight的大部分功能，但人的力量是有限的。所以我只做了批量向Redis中写入String的程序。  
做这个程序主要也是拿来试试手，为了做课表学习一下Javascript和WinUI3。所以..大概率不会更新新功能。


# Main feature
1. Bulk Add String.
2. Customize your application background.
3. __Write a .json file to Redis.__(View AppData/RedisKey.json for more information)  


1.批量添加String  
2.自定义应用背景（包饺子就是为了这盘醋）  
3.__将一个json格式的文件写入Redis。__（具体格式可以参照应用根目录下的AppData/RedisKey.json）


# Requirement
1. Redis server.
2. .net 6.0 framework desktop runtime.
