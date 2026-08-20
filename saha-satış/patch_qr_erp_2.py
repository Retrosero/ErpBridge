import re

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'r') as f:
    content = f.read()

# Inject local showBarcodeScanner state
state_injection = """
    var showBarcodeScanner by remember { mutableStateOf(false) }

    if (showBarcodeScanner) {
        BarcodeScannerDialog(
            onDismissRequest = { showBarcodeScanner = false },
            onBarcodeScanned = { code ->
                apiKey = code
                showBarcodeScanner = false
            },
            onSimulateScan = { code ->
                apiKey = code
                showBarcodeScanner = false
            }
        )
    }
"""
content = content.replace('var activeProgress by remember { mutableStateOf(0f) }', 'var activeProgress by remember { mutableStateOf(0f) }\n' + state_injection)


# Replace text fields trailing icon
trailing_icon = """trailingIcon = {
                                        IconButton(onClick = { showBarcodeScanner = true }) {
                                            Icon(androidx.compose.material.icons.Icons.Filled.QrCodeScanner, contentDescription = "QR Tarat")
                                        }
                                    }"""

pattern_goapp = re.compile(r'(label = \{ Text\("Aktivasyon Kodu", style = MaterialTheme.typography.bodySmall\) \},.*?)(modifier = Modifier.fillMaxWidth\(\),\s+textStyle = MaterialTheme.typography.bodySmall)(\s*\))', re.DOTALL)
content = pattern_goapp.sub(r'\1\2,\n                                    ' + trailing_icon + r'\3', content)

with open('app/src/main/java/com/example/ui/screens/ErpIntegrationScreen.kt', 'w') as f:
    f.write(content)
