$secret = "TaskManagement-Lab-SecretKey-AtLeast32Chars!!"

$header = @{
    alg = "HS256"
    typ = "JWT"
}

$payload = @{
    sub = "00000000-0000-0000-0000-000000000001"
    iss = "VK.Labs.TaskManagement"
    aud = "VK.Labs.TaskManagement"
    exp = [DateTimeOffset]::UtcNow.AddHours(1).ToUnixTimeSeconds()
    "vk.tenant.id" = "default-tenant"
    "vk.permissions" = @("project.write", "task.write")
}

function ConvertTo-Base64Url {
    param([byte[]]$bytes)
    $b64 = [Convert]::ToBase64String($bytes)
    return $b64.Replace('+', '-').Replace('/', '_').TrimEnd('=')
}

$headerJson = $header | ConvertTo-Json -Compress
$payloadJson = $payload | ConvertTo-Json -Compress

$headerB64Url = ConvertTo-Base64Url([System.Text.Encoding]::UTF8.GetBytes($headerJson))
$payloadB64Url = ConvertTo-Base64Url([System.Text.Encoding]::UTF8.GetBytes($payloadJson))

$dataToSign = "$headerB64Url.$payloadB64Url"

$hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($secret))
$signatureBytes = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($dataToSign))

$signatureB64Url = ConvertTo-Base64Url($signatureBytes)

$jwt = "$dataToSign.$signatureB64Url"
Write-Output $jwt
