# RegisterManager
这个TShock插件可以规范玩家角色名,防止某些特殊名字导致TShock指令无法选中他的问题.
此插件附带命令/usr,可以给予信任的管理员来管理玩家.解决原TShock命令/user权限过大.

命令:/usr 所需权限:RegisterManager

/usr add username password          -- 添加新用户</br>
/usr password username newpassword  -- 修改一个用户密码
/usr joinCheck                      -- 开启或关闭加入游戏时的用户名规范验证
/usr regperm                        -- 给予或收回guest用户组的注册权限

配置文件:tshock/RegisterManager.json
