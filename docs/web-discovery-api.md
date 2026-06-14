# 本地设备发现接口（前端对接）

> 面向 web 业务系统前端。背景与设计决策见 [`docs/adr/0009-add-local-device-discovery-service.md`](adr/0009-add-local-device-discovery-service.md) 与 [`docs/adr/0007-use-cloud-only-report-task-transport.md`](adr/adr/0007-use-cloud-only-report-task-transport.md)。

## 用途

cloud-only 重构后，前端不能直连本地 helper 发送任务，也**不再有 URL 协议**唤起。但前端发起打印任务前，必须知道「用户当前这台机器对应哪个 Cloud Device」——这个信息由 helper 提供的**只读本机 HTTP 发现服务**给出。

前端流程：

```
前端 → GET http://127.0.0.1:9294/discover  →  拿到 deviceId + 打印机列表
前端 → POST 云端业务接口（带 deviceId + printerName + 模板/数据 URL）→ 发起打印任务
云端 → 通过 cloud WebSocket 把任务投递给该 deviceId 的 helper
```

发现服务**只负责查询**，不接收任务、不改变状态。任务投递链路完全走云端。

## 端点

| 项 | 值 |
|---|---|
| 方法 | `GET` |
| 路径 | `/discover` |
| 地址 | `http://127.0.0.1:9294/discover`（默认端口；可由运维在 helper 的 `config.ini` 改 `DiscoveryPort`） |
| 请求体 | 无 |
| 查询参数 | 无 |

## 请求

浏览器跨域调用，**必须带 `Origin` 头**（浏览器自动加）。`Origin` 必须在 helper 的白名单里，否则返回 403。

```js
fetch('http://127.0.0.1:9294/discover')
  .then(r => r.json())
  .then(info => console.log(info.deviceId, info.printers));
```

无需任何鉴权头、token、cookie。

## CORS

发现服务对带 `Origin` 的请求返回 `Access-Control-Allow-Origin: <调用方 origin>`，并处理 `OPTIONS` 预检。前端用 `fetch`/`XMLHttpRequest` 直接调用即可，无需额外配置。

## 响应

### 200 OK

```json
{
  "deviceId": "3064bafe81ed4943bc3d73e4cc552d46",
  "deviceName": "DESKTOP-A0SRHNB",
  "defaultPrinter": "Microsoft Print to PDF",
  "printers": ["Microsoft Print to PDF", "HP LaserJet Pro M404"],
  "online": true
}
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `deviceId` | string | Cloud Device 全局唯一 ID（GUID，无连字符）。**这是发起云端打印任务时要带的设备号。** |
| `deviceName` | string | 设备显示名，默认为 Windows 计算机名，供用户辨认 |
| `defaultPrinter` | string \| null | 本机默认打印机名 |
| `printers` | string[] | 本机可用打印机名列表（本地枚举，与设备激活时上报云端的一致） |
| `online` | bool | helper 当前是否已连接云端 WebSocket。`true` 才能接收云端投递的任务；`false` 时即便发任务也到不了 |

> `printers` 返回的是**本地打印机名**，不是云端 `printerId`。前端发起任务时带 `printerName`，云端按 `(deviceId, printerName)` 解析。

### 错误响应

所有错误响应体均为 `{"error": "..."}`。

| 状态码 | 含义 | 触发条件 |
|---|---|---|
| 403 | `origin not allowed` | 调用方 `Origin` 不在白名单 |
| 404 | `not found` | 路径不是 `/discover` |
| 405 | `method not allowed` | 非 `GET`/`OPTIONS` |

### helper 未运行 / 端口不通

发现服务只在 helper 进程运行时存在。helper 未启动、端口被占、或调用地址端口与配置不符时，`fetch` 会因连接拒绝而抛 `TypeError`（网络错误）。前端应捕获并提示用户启动 helper，**不要**静默失败。

## Origin 白名单

helper 用 `config.ini` 的 `DiscoveryAllowedOrigins` 控制哪些前端来源可以调用，逗号或分号分隔：

```ini
DiscoveryAllowedOrigins = 127.0.0.1,http://biz.example.com
```

匹配规则（任一命中即放行）：

1. 与浏览器 `Origin` 完全相等
2. **host 匹配**：配置项作为 host（或从中解析出的 host）等于浏览器 `Origin` 的 host，忽略 scheme 与端口
   - 配 `127.0.0.1` → 命中 `http://127.0.0.1`、`http://127.0.0.1:8080`、`http://127.0.0.1:3000` 等
   - 配 `http://biz.example.com` → 命中 `http://biz.example.com:80`、`https://biz.example.com` 等
3. **无 `Origin` 头**（curl、同机非浏览器工具调用）→ 一律放行

默认值 `127.0.0.1`，即默认只允许本机来源。生产环境把业务系统域名加进去。空白名单 = 拒绝所有浏览器调用。

## 前端调用示例

```js
async function discoverDevice(timeoutMs = 2000) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const res = await fetch('http://127.0.0.1:9294/discover', { signal: controller.signal });
    if (!res.ok) {
      throw new Error(`discovery rejected: ${res.status}`);
    }
    return await res.json(); // { deviceId, deviceName, defaultPrinter, printers, online }
  } catch (err) {
    // helper 未运行 / 端口不通 / 超时
    throw new Error('报表助手未启动，请先启动后重试');
  } finally {
    clearTimeout(timer);
  }
}

// 拿到 deviceId 后，调云端业务接口发起打印
const device = await discoverDevice();
if (!device.online) {
  throw new Error('报表助手未连接云端，请检查网络或联系管理员');
}
await fetch('/api/cloud-print/tasks', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    deviceId: device.deviceId,
    printerName: device.defaultPrinter,  // 或让用户从 device.printers 选
    templateUrl: '...',
    dataUrl: '...',
    cmd: 'print'
  })
});
```

> 建议加超时（helper 异常时不让前端干等）。打印任务状态由云端接口返回，**不**在本服务范围。

## curl 验证

非浏览器调用无 `Origin` 头，直接放行，便于联调：

```bash
curl http://127.0.0.1:9294/discover
```

模拟浏览器带 Origin：

```bash
curl -H "Origin: http://127.0.0.1:8080" http://127.0.0.1:9294/discover
```

## 运维配置

helper 端所有可调项在 `config.ini` 的 `[App]` 节，修改后通过托盘「本地发现」窗口点「应用并重启」生效（或重启 helper）：

| 键 | 默认 | 说明 |
|---|---|---|
| `DiscoveryEnabled` | `True` | 是否启动发现服务 |
| `DiscoveryPort` | `9294` | 监听端口。改了端口，前端调用地址与 urlacl 也要同步（urlacl 由安装包注册默认端口，自定义端口需运维另行 `netsh http add urlacl`） |
| `DiscoveryAllowedOrigins` | `127.0.0.1` | 允许的前端来源，逗号/分号分隔 |

## 限制与边界

- 仅 `GET /discover`，无其他端点
- 不接收任何任务、打印指令、状态变更请求（任务走云端）
- 不做用户身份校验——靠 `127.0.0.1` 绑定 + Origin 白名单限制可见性
- 只反映「helper 当前视角的本机」，不知道其他设备；多设备场景由前端在云端侧管理
