﻿using System.Collections.Generic;
using System.Linq;

namespace Ritsukage.Library.FFXIV.Data
{
    public static class Zone
    {
        static readonly SortedList<int, string> Data = new()
        {
            { 4015, "万相森国" },
            { 3769, "万魔殿正门" },
            { 3797, "万魔殿边境下层" },
            { 3798, "万魔殿边境水道" },
            { 2130, "不获岛" },
            { 32, "东拉诺西亚" },
            { 44, "东萨纳兰" },
            { 30, "中拉诺西亚" },
            { 468, "中枢区" },
            { 1811, "中桅塔大厅" },
            { 1812, "中桅塔房间" },
            { 43, "中萨纳兰" },
            { 2340, "乌兹奈藏宝运河" },
            { 2485, "乌兹奈藏宝运河神殿" },
            { 2927, "乌尔达哈商会馆接待室" },
            { 41, "乌尔达哈来生回廊" },
            { 40, "乌尔达哈现世回廊" },
            { 2864, "乐欲之所瓯博讷修道院" },
            { 2310, "九霄云舍" },
            { 4039, "云使宫客房" },
            { 1647, "云冠群岛" },
            { 406, "云廊" },
            { 1847, "亚历山大之心" },
            { 1841, "亚历山大之息" },
            { 1835, "亚历山大之眼" },
            { 1853, "亚历山大之魂" },
            { 2041, "亚历山大机神城" },
            { 2985, "亚马乌罗提" },
            { 1834, "亡灵府邸闹鬼庄园" },
            { 3576, "人偶军事基地" },
            { 2300, "伊修加德基础层" },
            { 2327, "伊修加德教皇厅" },
            { 2301, "伊修加德砥柱层" },
            { 2956, "伊尔美格" },
            { 3214, "伊甸内核" },
            { 3470, "伊甸甲板" },
            { 3818, "休息室" },
            { 2545, "优雷卡丰水之地" },
            { 2414, "优雷卡常风之地" },
            { 2462, "优雷卡恒冰之地" },
            { 2530, "优雷卡涌火之地" },
            { 3736, "佐特塔" },
            { 4023, "作战会议室" },
            { 2083, "供奉洞" },
            { 694, "兽斗间" },
            { 2336, "决斗裁判场" },
            { 4198, "创生树隔离层" },
            { 1804, "利奥法德的房间" },
            { 28, "利姆萨·罗敏萨上层甲板" },
            { 29, "利姆萨·罗敏萨下层甲板" },
            { 3926, "前关门" },
            { 2371, "剧场艇初见号舰桥" },
            { 2370, "剧场艇初见号道具间" },
            { 1374, "加尔提诺平原周边遗迹群" },
            { 3710, "加雷马" },
            { 3477, "努力号" },
            { 46, "北萨纳兰" },
            { 112, "十二神大圣堂" },
            { 3534, "南方博兹雅战线" },
            { 45, "南萨纳兰" },
            { 3713, "厄尔庇斯" },
            { 1800, "双蛇党军营" },
            { 478, "古代人迷宫" },
            { 3711, "叹息海" },
            { 733, "后桅旅店" },
            { 4032, "后营门" },
            { 459, "呼啸眼外围" },
            { 361, "呼啸眼石塔群" },
            { 4045, "咒发风塔" },
            { 50, "喀恩埋没圣堂" },
            { 392, "圣人泪" },
            { 2337, "圣恩达利姆神学院" },
            { 2034, "圣茉夏娜植物园" },
            { 2586, "基姆利特暗区" },
            { 3442, "基姆利特防线" },
            { 2407, "基拉巴尼亚山区" },
            { 2408, "基拉巴尼亚湖区" },
            { 2406, "基拉巴尼亚边区" },
            { 58, "塔姆·塔拉墓园" },
            { 1792, "塞尔法特尔溪谷" },
            { 3425, "复制工厂废墟" },
            { 2979, "多恩美格禁园" },
            { 2813, "多玛飞地" },
            { 4022, "夜游魂总部" },
            { 3487, "大冰河" },
            { 3228, "大图帕萨礼拜堂" },
            { 2448, "大草原猎场" },
            { 2775, "天之御柱" },
            { 3712, "天外天垓" },
            { 430, "天幕魔导城" },
            { 2339, "天母寝宫地下祭坛" },
            { 230, "天狼星灯塔" },
            { 3435, "天穹街" },
            { 2548, "天青斗场" },
            { 2411, "太阳神草原" },
            { 2372, "失落之都拉巴纳斯塔" },
            { 3542, "失落的遗迹" },
            { 3018, "奇坦那神影洞" },
            { 2178, "奇点反应堆" },
            { 359, "奥·哥摩罗火口神殿" },
            { 3597, "女王古殿" },
            { 2297, "妖歌海" },
            { 3218, "妖灵王舞场" },
            { 3568, "始皇宝座" },
            { 1815, "娜娜莫大风车大厅" },
            { 1816, "娜娜莫大风车房间" },
            { 3385, "宇宙宫" },
            { 2955, "安穆·艾兰" },
            { 3219, "完璧王座" },
            { 2295, "宝物殿" },
            { 4196, "寄生生物隔离层" },
            { 418, "密约之塔" },
            { 1334, "对利维亚桑双体船" },
            { 4146, "寻因星晶镜" },
            { 2483, "封闭圣塔黎铎拉纳大灯塔" },
            { 496, "尘封秘岩" },
            { 2527, "尼姆浮游遗迹风格运动设施" },
            { 3686, "尽头的终点" },
            { 2801, "岩燕庙" },
            { 3017, "差分闭合宇宙" },
            { 2833, "巴儿达木霸道" },
            { 4118, "巴别塔" },
            { 1759, "巴哈姆特大迷宫" },
            { 1857, "巴埃萨长城" },
            { 36, "布雷福洛克斯野营地" },
            { 493, "希尔科斯塔" },
            { 2982, "希尔科斯孪晶塔" },
            { 3221, "希尔科斯峡谷" },
            { 3647, "希望之炮台：“塔”" },
            { 2936, "帕戈尔赞" },
            { 1803, "帕洛克系留基地" },
            { 260, "帝国南方堡" },
            { 2799, "帝国河畔堡" },
            { 3581, "帝国海上基地干船坞" },
            { 2665, "帝国白山堡" },
            { 1665, "幻卡对局室" },
            { 63, "库尔札斯中央高地" },
            { 2200, "库尔札斯西部高地" },
            { 2410, "延夏" },
            { 1660, "开发室" },
            { 1731, "弥达斯之担" },
            { 1708, "弥达斯之拳" },
            { 1723, "弥达斯之臂" },
            { 1714, "弥达斯之袖" },
            { 2847, "归燕馆" },
            { 1868, "影之国" },
            { 2357, "德尔塔幻境1" },
            { 2358, "德尔塔幻境2" },
            { 2359, "德尔塔幻境3" },
            { 2360, "德尔塔幻境4" },
            { 2090, "忆罪宫" },
            { 1801, "恒辉队军营" },
            { 3222, "悬挂公馆起居室" },
            { 4111, "惊奇百宝城" },
            { 1960, "惨境号" },
            { 1645, "戈耳狄俄斯之担" },
            { 1628, "戈耳狄俄斯之拳" },
            { 1638, "戈耳狄俄斯之臂" },
            { 1633, "戈耳狄俄斯之袖" },
            { 1390, "戒律之茧" },
            { 4034, "所思大书院禁书库" },
            { 3662, "扎杜诺尔高原" },
            { 61, "托托·拉克千狱" },
            { 2265, "抑制绝境p1t6" },
            { 2256, "抑制绝境s1t7" },
            { 2266, "抑制绝境z1t9" },
            { 404, "披雪大冰壁" },
            { 3620, "拉克汕城" },
            { 2957, "拉凯提卡大森林" },
            { 3707, "拉札罕" },
            { 31, "拉诺西亚低地" },
            { 350, "拉诺西亚外地" },
            { 34, "拉诺西亚高地" },
            { 1302, "拘束舰外围" },
            { 1429, "接待室" },
            { 351, "提督室" },
            { 67, "摩杜纳" },
            { 37, "放浪神古神殿" },
            { 2367, "斯卡拉遗迹" },
            { 3783, "斯提格玛四" },
            { 4043, "无名岛" },
            { 2151, "无尽苍空" },
            { 1399, "无尽轮回剧场" },
            { 2148, "无限回廊" },
            { 128, "无限城古堡" },
            { 125, "无限城市街古迹" },
            { 49, "日影地修炼所" },
            { 3706, "旧萨雷安" },
            { 2737, "时空狭缝" },
            { 3378, "昂萨哈凯尔" },
            { 2707, "星导寺" },
            { 2725, "普西幻境1" },
            { 2736, "普西幻境2" },
            { 1431, "暗之世界" },
            { 3511, "暗影决战诺弗兰特" },
            { 3596, "暗黑领域" },
            { 2214, "暮卫塔" },
            { 2549, "曼德维尔魔导方城" },
            { 3684, "月球深处" },
            { 2413, "望海楼" },
            { 4100, "末世终迹" },
            { 3427, "机械遗迹坑道" },
            { 3759, "极北造物院" },
            { 548, "栖木旅馆" },
            { 52, "格里达尼亚新街" },
            { 53, "格里达尼亚旧街" },
            { 2997, "格鲁格火山" },
            { 3229, "梦羽宝境" },
            { 3644, "梦羽宝殿" },
            { 47, "樵鸣洞" },
            { 379, "次元缝隙迎宾室" },
            { 1887, "欧米茄控制室" },
            { 3817, "正厅" },
            { 2081, "武神斗技场" },
            { 1793, "死者宫殿" },
            { 3685, "母水晶" },
            { 1799, "水城宝物库" },
            { 2951, "水晶都" },
            { 3050, "水滩村" },
            { 356, "沙之家" },
            { 35, "沙斯塔夏溶洞" },
            { 617, "沙钟旅亭" },
            { 64, "泽梅尔要塞" },
            { 425, "海雾村" },
            { 1157, "海雾村个人房间" },
            { 1101, "海雾村私人公馆" },
            { 1102, "海雾村私人别墅" },
            { 1100, "海雾村私人小屋" },
            { 1227, "海雾村部队工房" },
            { 3216, "涅柔斯海渊" },
            { 2952, "游末邦" },
            { 3595, "游末邦监狱" },
            { 3469, "灰扬旅路" },
            { 4167, "灿烂神域阿格莱亚" },
            { 357, "炎帝陵" },
            { 2805, "烈士庵" },
            { 3571, "燕鸥崖" },
            { 3570, "燕鸥崖海湾" },
            { 4180, "特罗亚宫廷" },
            { 386, "狮鹫大桥" },
            { 2496, "狱之底" },
            { 2762, "狱之盖" },
            { 358, "狼狱停船场" },
            { 1664, "狼狱演习场" },
            { 4249, "猛毒生物隔离层" },
            { 3590, "玛托雅工作室" },
            { 2036, "玛托雅的洞穴" },
            { 2954, "珂露西亚岛" },
            { 3468, "甘多夫雷平原" },
            { 3478, "甘戈斯" },
            { 4250, "生命奥秘研究层" },
            { 2082, "田园郡" },
            { 2354, "白帝竹林" },
            { 2412, "白银乡" },
            { 2270, "白银乡个人房间" },
            { 1894, "白银乡私人公馆" },
            { 1895, "白银乡私人别墅" },
            { 1893, "白银乡私人小屋" },
            { 2271, "白银乡部队工房" },
            { 1813, "百合岭大厅" },
            { 1814, "百合岭房间" },
            { 3694, "皓天炉舍大厅" },
            { 3695, "皓天炉舍房间" },
            { 481, "石之家" },
            { 401, "石卫塔" },
            { 1377, "破舰岛" },
            { 1363, "神判古树" },
            { 346, "神勇队司令室" },
            { 2403, "神拳痕" },
            { 2335, "神殿骑士团总骑士长室" },
            { 4024, "神门之间" },
            { 1742, "禁忌城邦玛哈" },
            { 2449, "禁绝幻想" },
            { 2320, "福尔唐伯爵府" },
            { 695, "福隆戴尔药学院儿科病房" },
            { 3663, "究极救世者g型战斗甲板" },
            { 4139, "穹顶皓天" },
            { 3692, "穹顶皓天个人房间" },
            { 3690, "穹顶皓天私人公馆" },
            { 3691, "穹顶皓天私人别墅" },
            { 3689, "穹顶皓天私人小屋" },
            { 3693, "穹顶皓天部队工房" },
            { 3225, "空无大地" },
            { 3770, "笑笑镇" },
            { 469, "第一舰桥" },
            { 2031, "索姆阿尔灵峰" },
            { 2779, "紫水宫" },
            { 2272, "红梅御殿大厅" },
            { 2273, "红梅御殿房间" },
            { 2409, "红玉海" },
            { 2851, "终末焦土" },
            { 2715, "结晶化空间" },
            { 3635, "绝望庭园" },
            { 462, "罗塔诺海" },
            { 2299, "美神地下神殿" },
            { 4031, "翁法洛斯" },
            { 2002, "翻云雾海" },
            { 2862, "艾欧泽亚同盟军大本营" },
            { 152, "艾欧泽亚地下空间" },
            { 347, "芙蓉圆桌" },
            { 360, "荆棘之园" },
            { 1740, "荣誉野" },
            { 3709, "萨维奈岛" },
            { 426, "薰衣草苗圃" },
            { 1159, "薰衣草苗圃个人房间" },
            { 1107, "薰衣草苗圃私人公馆" },
            { 1108, "薰衣草苗圃私人别墅" },
            { 1106, "薰衣草苗圃私人小屋" },
            { 1229, "薰衣草苗圃部队工房" },
            { 2510, "蛇神大社" },
            { 33, "西拉诺西亚" },
            { 2717, "西格玛幻境1" },
            { 2718, "西格玛幻境2" },
            { 2719, "西格玛幻境3" },
            { 2720, "西格玛幻境4" },
            { 42, "西萨纳兰" },
            { 3223, "观星室" },
            { 3471, "试炼前室" },
            { 4098, "诗想空间" },
            { 1304, "诸神黄昏级三号舰作战室" },
            { 1305, "诸神黄昏级三号舰第一舰桥" },
            { 1303, "诸神黄昏级三号舰舰体中央" },
            { 1407, "诸神黄昏级六号舰再生控制区" },
            { 1408, "诸神黄昏级六号舰第一舰桥" },
            { 1406, "诸神黄昏级六号舰舰体中央" },
            { 1410, "诸神黄昏级四号舰第一舰桥" },
            { 466, "诸神黄昏级拘束舰" },
            { 2498, "贝拉哈迪亚遗迹风格运动设施" },
            { 2296, "超越技术研究所" },
            { 2313, "轻羽斗场" },
            { 4135, "边境之狱深层" },
            { 2038, "迦巴勒幻想图书馆" },
            { 3708, "迷津" },
            { 3486, "通信雷波塔控制室" },
            { 2392, "醴泉神社" },
            { 2391, "醴泉神社神道" },
            { 1484, "金碟游乐场" },
            { 48, "铜铃铜山" },
            { 354, "银胄团总长室" },
            { 3696, "闹鬼盛宴" },
            { 4154, "阿尔扎达尔海底遗迹群" },
            { 3007, "阿尼德罗学院" },
            { 3467, "阿尼德罗追忆馆" },
            { 2100, "阿巴拉提亚云海" },
            { 2691, "阿拉米格" },
            { 2294, "阿拉米格王宫" },
            { 2709, "阿拉米格王宫屋顶庭园" },
            { 2708, "阿拉米格王立飞空艇着陆场" },
            { 3217, "阿特拉斯山顶" },
            { 1500, "陆行鸟广场" },
            { 153, "陌迪翁牢狱" },
            { 465, "陨石勘查坑深层" },
            { 464, "陨石勘查坑表层" },
            { 1301, "陨石背阴地" },
            { 2589, "隐塞" },
            { 467, "隔离壁" },
            { 4038, "零的领域" },
            { 2953, "雷克兰德" },
            { 59, "静语庄园" },
            { 2088, "颠倒塔" },
            { 3139, "马利卡大井" },
            { 427, "高脚孤丘" },
            { 1158, "高脚孤丘个人房间" },
            { 1104, "高脚孤丘私人公馆" },
            { 1105, "高脚孤丘私人别墅" },
            { 1103, "高脚孤丘私人小屋" },
            { 1228, "高脚孤丘部队工房" },
            { 4040, "魔人的藏身之处" },
            { 2179, "魔大陆中枢" },
            { 2101, "魔大陆阿济兹拉" },
            { 2147, "魔科学研究所" },
            { 2181, "魔航船虚无方舟" },
            { 2499, "黄金大桥" },
            { 2404, "黄金港" },
            { 65, "黄金谷" },
            { 2298, "黄金阁" },
            { 2451, "黎铎拉纳大瀑布" },
            { 1802, "黑涡团军营" },
            { 55, "黑衣森林东部林区" },
            { 54, "黑衣森林中央林区" },
            { 57, "黑衣森林北部林区" },
            { 56, "黑衣森林南部林区" },
            { 2958, "黑风海" },
            { 2001, "龙堡内陆低地" },
            { 2000, "龙堡参天高地" },
            { 2050, "龙巢神殿" },
            { 1409, "龙炎核心" },
        };

        public static KeyValuePair<int, string>[] SearchZoneID(string zone)
            => Data.Where(x => x.Value.Contains(zone))?.ToArray();

        public static int GetZoneID(string zone)
            => SearchZoneID(zone)?.FirstOrDefault().Key ?? 0;

        public static string GetZoneName(int zoneID)
            => Data.GetValueOrDefault(zoneID, string.Empty);
    }
}
