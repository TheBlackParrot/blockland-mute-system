// http://forum.blockland.us/index.php?topic=207416.msg5764123#msg5764123
function isInt(%val) {
	return %val $= (%val << 0);
}