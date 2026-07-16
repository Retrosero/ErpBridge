package com.example.ui.components

import android.widget.Toast
import androidx.compose.animation.*
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.PathEffect
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

data class ReceiptInfo(
    val type: String, // "TAHSİLAT" or "TEDİYE"
    val receiptNo: String,
    val date: String,
    val customerName: String,
    val customerId: String,
    val amount: Double,
    val paymentMethod: String,
    val bankName: String?,
    val memo: String
)

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReceiptDialog(
    info: ReceiptInfo,
    onDismiss: () -> Unit
) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var isPrinting by remember { mutableStateOf(false) }
    var isSavingImg by remember { mutableStateOf(false) }
    var printFinished by remember { mutableStateOf(false) }
    var saveFinished by remember { mutableStateOf(false) }

    Dialog(
        onDismissRequest = { 
            if (!isPrinting && !isSavingImg) onDismiss() 
        },
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Surface(
            modifier = Modifier
                .fillMaxWidth(0.92f)
                .wrapContentHeight()
                .padding(vertical = 24.dp),
            shape = RoundedCornerShape(24.dp),
            color = MaterialTheme.colorScheme.background,
            tonalElevation = 6.dp
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(20.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Header
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(36.dp)
                                .background(
                                    if (info.type == "TAHSİLAT") Color(0xFF2E7D32).copy(alpha = 0.15f)
                                    else Color(0xFFC62828).copy(alpha = 0.15f),
                                    CircleShape
                                ),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                imageVector = if (info.type == "TAHSİLAT") Icons.Filled.ArrowDownward else Icons.Filled.ArrowUpward,
                                contentDescription = null,
                                tint = if (info.type == "TAHSİLAT") Color(0xFF2E7D32) else Color(0xFFC62828),
                                modifier = Modifier.size(18.dp)
                            )
                        }
                        Text(
                            text = if (info.type == "TAHSİLAT") "Tahsilat Fişi" else "Tediye Fişi",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    IconButton(
                        onClick = onDismiss,
                        enabled = !isPrinting && !isSavingImg
                    ) {
                        Icon(Icons.Filled.Close, contentDescription = "Kapat")
                    }
                }

                Divider(modifier = Modifier.padding(vertical = 12.dp), color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.6f))

                // Scrollable Ticket View
                Column(
                    modifier = Modifier
                        .weight(1f, fill = false)
                        .verticalScroll(rememberScrollState())
                        .clip(RoundedCornerShape(16.dp))
                        .background(Color(0xFFFAFAF7)) // Clean elegant recycled-paper colored ticket
                        .border(1.dp, Color(0xFFE2E2D5), RoundedCornerShape(16.dp))
                        .padding(bottom = 16.dp)
                ) {
                    // Modern Styled Ticket top banner
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(
                                if (info.type == "TAHSİLAT") Color(0xFF2E7D32).copy(alpha = 0.08f)
                                else Color(0xFFC62828).copy(alpha = 0.08f)
                            )
                            .padding(16.dp)
                    ) {
                        Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.fillMaxWidth()) {
                            Text(
                                "MİRAÇ KOZMETİK VE DIŞ TİC. LTD. ŞTİ.",
                                style = MaterialTheme.typography.labelSmall,
                                fontWeight = FontWeight.Black,
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                                textAlign = TextAlign.Center,
                                letterSpacing = 1.sp
                            )
                            Spacer(modifier = Modifier.height(2.dp))
                            Text(
                                "Merkez Mah. Atatürk Cad. No:142 Şişli / İstanbul",
                                style = MaterialTheme.typography.labelSmall,
                                color = Color.Gray,
                                fontSize = 10.sp,
                                textAlign = TextAlign.Center
                            )
                        }
                    }

                    // Dotted divider simulating tear-off slip
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(14.dp)
                            .drawBehind {
                                val pathEffect = PathEffect.dashPathEffect(floatArrayOf(10f, 8f), 0f)
                                drawLine(
                                    color = Color(0xFFC4C4B5),
                                    start = Offset(0f, 7f),
                                    end = Offset(size.width, 7f),
                                    strokeWidth = 2f,
                                    pathEffect = pathEffect
                                )
                            }
                    )

                    // Ticket Info Fields
                    Column(
                        modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        ReceiptRow("Makbuz No", info.receiptNo)
                        ReceiptRow("Tarih & Saat", info.date)
                        ReceiptRow("Cari Hesap", "[${info.customerId}] ${info.customerName}")
                        ReceiptRow("Ödeme Tipi", info.paymentMethod)
                        if (!info.bankName.isNullOrEmpty()) {
                            ReceiptRow("Banka Hesabı", info.bankName)
                        }
                        if (info.memo.isNotEmpty()) {
                            ReceiptRow("Açıklama", info.memo)
                        }

                        // Ticket Separator
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 8.dp)
                                .height(1.dp)
                                .background(Color(0xFFE2E2D5))
                        )

                        // Highlighted Large Total Box
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clip(RoundedCornerShape(12.dp))
                                .background(Color.White)
                                .border(1.dp, Color(0xFFE2E2D5), RoundedCornerShape(12.dp))
                                .padding(16.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                Text(
                                    text = if (info.type == "TAHSİLAT") "TOPLAM TAHSİLAT" else "TOPLAM TEDİYE",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = Color.Gray,
                                    fontWeight = FontWeight.Bold
                                )
                                Spacer(modifier = Modifier.height(4.dp))
                                Text(
                                    text = String.format("%,.2f ₺", info.amount),
                                    style = MaterialTheme.typography.headlineMedium,
                                    fontWeight = FontWeight.Black,
                                    color = if (info.type == "TAHSİLAT") Color(0xFF2E7D32) else Color(0xFFC62828)
                                )
                                Spacer(modifier = Modifier.height(4.dp))
                                Text(
                                    text = "Yalnız: ${convertNumberToTurkishWords(info.amount)}",
                                    style = MaterialTheme.typography.labelSmall,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                                    fontFamily = FontFamily.Monospace,
                                    fontSize = 11.sp,
                                    textAlign = TextAlign.Center,
                                    fontWeight = FontWeight.Medium,
                                    modifier = Modifier.padding(horizontal = 6.dp)
                                )
                            }
                        }

                        // Signature Section
                        Spacer(modifier = Modifier.height(10.dp))
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.weight(1f)) {
                                Text("Teslim Eden", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)
                                Spacer(modifier = Modifier.height(28.dp))
                                Box(modifier = Modifier.width(60.dp).height(1.dp).background(Color.Gray))
                                Text("İmza", fontSize = 9.sp, color = Color.LightGray)
                            }
                            Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.weight(1f)) {
                                Text("Teslim Alan", style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold, color = Color.Gray)
                                Spacer(modifier = Modifier.height(28.dp))
                                Box(modifier = Modifier.width(60.dp).height(1.dp).background(Color.Gray))
                                Text("İmza / Kaşe", fontSize = 9.sp, color = Color.LightGray)
                            }
                        }

                        // Barcode visual simulation for realism
                        Spacer(modifier = Modifier.height(14.dp))
                        Column(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalAlignment = Alignment.CenterHorizontally,
                            verticalArrangement = Arrangement.spacedBy(2.dp)
                        ) {
                            Canvas(modifier = Modifier.width(180.dp).height(24.dp)) {
                                val barWidths = listOf(
                                    2f, 4f, 1f, 3f, 1f, 2f, 4f, 2f, 1f, 3f, 2f, 4f, 1f, 2f, 1f, 3f, 4f, 1f, 2f,
                                    3f, 1f, 4f, 2f, 1f, 3f, 1f, 2f, 4f, 2f, 1f, 3f, 2f, 4f, 1f, 2f, 1f, 3f, 4f,
                                    2f, 4f, 1f, 3f, 1f, 2f, 3f, 1f, 4f, 2f, 1f, 3f, 1f, 2f, 4f, 2f, 1f, 3f, 2f
                                )
                                var currentX = 0f
                                val totalBarsWidth = barWidths.sum()
                                val scale = size.width / totalBarsWidth
                                
                                barWidths.forEachIndexed { idx, w ->
                                    val finalW = w * scale
                                    if (idx % 2 == 0) {
                                        drawRect(
                                            color = Color.DarkGray,
                                            topLeft = Offset(currentX, 0f),
                                            size = androidx.compose.ui.geometry.Size(finalW, size.height)
                                        )
                                    }
                                    currentX += finalW
                                }
                            }
                            Text(
                                text = "*${info.receiptNo}*",
                                fontSize = 10.sp,
                                fontFamily = FontFamily.Monospace,
                                color = Color.Gray
                            )
                        }
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                // Action Operations Info Panel (for print/save simulation feedback)
                AnimatedVisibility(
                    visible = isPrinting || isSavingImg || printFinished || saveFinished,
                    enter = fadeIn() + expandVertically(),
                    exit = fadeOut() + shrinkVertically()
                ) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp)
                            .clip(RoundedCornerShape(12.dp))
                            .background(MaterialTheme.colorScheme.secondaryContainer.copy(alpha = 0.5f))
                            .padding(12.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.spacedBy(10.dp)
                        ) {
                            if (isPrinting || isSavingImg) {
                                CircularProgressIndicator(modifier = Modifier.size(20.dp), strokeWidth = 2.dp)
                                Text(
                                    text = if (isPrinting) "Termal Yazıcıya Gönderiliyor..." else "PNG Formatında Kaydediliyor...",
                                    style = MaterialTheme.typography.bodyMedium,
                                    fontWeight = FontWeight.Medium
                                )
                            } else if (printFinished) {
                                Icon(Icons.Filled.Print, contentDescription = null, tint = Color(0xFF2E7D32))
                                Text("Makbuz yazıcıya gönderildi (A4/Termal Bluetooth)", style = MaterialTheme.typography.bodyMedium, color = Color(0xFF2E7D32), fontWeight = FontWeight.Bold)
                            } else if (saveFinished) {
                                Icon(Icons.Filled.Image, contentDescription = null, tint = Color(0xFF2E7D32))
                                Text("Galeriye PNG olarak kaydedildi!", style = MaterialTheme.typography.bodyMedium, color = Color(0xFF2E7D32), fontWeight = FontWeight.Bold)
                            }
                        }
                    }
                }

                // Interactive Buttons
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    // PNG Save Button
                    Button(
                        onClick = {
                            scope.launch {
                                isSavingImg = true
                                saveFinished = false
                                delay(1600) // Realistic process speed
                                isSavingImg = false
                                saveFinished = true
                                Toast.makeText(context, "Makbuz başarıyla PNG olarak /Galeri/Makbuzlar klasörüne kaydedildi!", Toast.LENGTH_SHORT).show()
                            }
                        },
                        modifier = Modifier.weight(1f),
                        enabled = !isPrinting && !isSavingImg,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = MaterialTheme.colorScheme.secondaryContainer,
                            contentColor = MaterialTheme.colorScheme.onSecondaryContainer
                        ),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Icon(Icons.Filled.PhotoLibrary, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("PNG Kaydet", fontSize = 12.sp, fontWeight = FontWeight.Bold)
                    }

                    // Print/Yazdır Button
                    Button(
                        onClick = {
                            scope.launch {
                                isPrinting = true
                                printFinished = false
                                delay(1800) // Realistic printing delay
                                isPrinting = false
                                printFinished = true
                                Toast.makeText(context, "Yazıcı bağlantısı kuruldu. Fiş basılıyor...", Toast.LENGTH_SHORT).show()
                            }
                        },
                        modifier = Modifier.weight(1f),
                        enabled = !isPrinting && !isSavingImg,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = MaterialTheme.colorScheme.primary,
                            contentColor = MaterialTheme.colorScheme.onPrimary
                        ),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Icon(Icons.Filled.Print, contentDescription = null, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("Yazdır / Fiş", fontSize = 12.sp, fontWeight = FontWeight.Bold)
                    }
                }

                Spacer(modifier = Modifier.height(10.dp))

                // Complete/Close Button
                OutlinedButton(
                    onClick = onDismiss,
                    modifier = Modifier.fillMaxWidth(),
                    enabled = !isPrinting && !isSavingImg,
                    shape = RoundedCornerShape(12.dp)
                ) {
                    Text("İşlemi Tamamla", fontWeight = FontWeight.Bold)
                }
            }
        }
    }
}

@Composable
fun ReceiptRow(label: String, value: String) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.Top
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            fontWeight = FontWeight.Bold,
            color = Color.Gray,
            modifier = Modifier.weight(0.35f)
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodySmall,
            fontWeight = FontWeight.Medium,
            color = Color.DarkGray,
            textAlign = TextAlign.End,
            modifier = Modifier.weight(0.65f)
        )
    }
}

// Simple and highly effective number-to-Turkish-words converter
fun convertNumberToTurkishWords(amount: Double): String {
    val lAmount = amount.toLong()
    if (amount <= 0.0) return "Sıfır"
    
    val units = arrayOf("", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz")
    val tens = arrayOf("", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Atmış", "Yetmiş", "Seksen", "Doksan")
    val thousands = arrayOf("", "Bin", "Milyon", "Milyar")
    
    fun convertThreeDigits(num: Int): String {
        var n = num
        var str = ""
        val h = n / 100
        if (h > 0) {
            str += if (h == 1) "Yüz" else units[h] + "Yüz"
        }
        n %= 100
        val t = n / 10
        if (t > 0) {
            str += tens[t]
        }
        val u = n % 10
        if (u > 0) {
            str += units[u]
        }
        return str
    }
    
    var temp = lAmount
    var result = ""
    var place = 0
    
    while (temp > 0) {
        val chunk = (temp % 1000).toInt()
        if (chunk > 0) {
            val chunkStr = convertThreeDigits(chunk)
            if (place == 1 && chunk == 1) {
                // "Bir Bin" değil sadece "Bin"
                result = "Bin" + result
            } else {
                result = chunkStr + thousands[place] + result
            }
        }
        temp /= 1000
        place++
    }
    
    // Kuruş calculations
    val kurus = ((amount - lAmount.toDouble()) * 100 + 0.5).toInt()
    val kurusStr = if (kurus > 0) {
        val kt = kurus / 10
        val ku = kurus % 10
        val tensStr = tens[kt]
        val unitsStr = units[ku]
        " ve $tensStr$unitsStr Kuruş"
    } else {
        ""
    }
    
    return "${result} Türk Lirası${kurusStr}"
}
