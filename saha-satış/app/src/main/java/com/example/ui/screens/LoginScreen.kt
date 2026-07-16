package com.example.ui.screens

import android.widget.Toast
import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import com.example.data.database.DatabaseProvider
import com.example.data.database.UserEntity
import com.example.ui.components.FieldPrimaryButton
import com.example.ui.components.FieldSecondaryButton
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

@OptIn(ExperimentalAnimationApi::class)
@Composable
fun LoginScreen(navController: NavController) {
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()

    var isLoginTab by remember { mutableStateOf(true) }

    // Login Fields
    var loginUsername by remember { mutableStateOf("") }
    var loginPassword by remember { mutableStateOf("") }

    // Register Fields
    var regFullName by remember { mutableStateOf("") }
    var regUsername by remember { mutableStateOf("") }
    var regEmail by remember { mutableStateOf("") }
    var regPassword by remember { mutableStateOf("") }
    var regPasswordConfirm by remember { mutableStateOf("") }

    var passwordVisible by remember { mutableStateOf(false) }
    var regPasswordVisible by remember { mutableStateOf(false) }

    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    val scrollState = rememberScrollState()

    // Ensure double-check pre-populated user is loaded
    LaunchedEffect(Unit) {
        withContext(Dispatchers.IO) {
            val db = DatabaseProvider.getDatabase(context)
            val users = db.userDao().getAllUsers()
            if (users.isEmpty()) {
                // Register a useful default demo account: admin / admin
                db.userDao().insertUser(
                    UserEntity(
                        username = "admin",
                        passwordHash = "admin",
                        fullName = "Saha Yöneticisi",
                        email = "admin@fieldforce.com",
                        isLoggedIn = false
                    )
                )
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
            .windowInsetsPadding(WindowInsets.safeDrawing)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(scrollState)
                .padding(24.dp),
            verticalArrangement = Arrangement.Top,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Spacer(modifier = Modifier.height(24.dp))

            // Beautiful branding header
            Icon(
                imageVector = Icons.Filled.ShoppingCart,
                contentDescription = null,
                modifier = Modifier
                    .size(64.dp)
                    .background(
                        color = MaterialTheme.colorScheme.primaryContainer,
                        shape = RoundedCornerShape(16.dp)
                    )
                    .padding(12.dp),
                tint = MaterialTheme.colorScheme.primary
            )

            Spacer(modifier = Modifier.height(16.dp))

            Text(
                "Sipariş Cepte",
                style = MaterialTheme.typography.headlineMedium,
                fontWeight = FontWeight.ExtraBold,
                color = MaterialTheme.colorScheme.primary,
                textAlign = TextAlign.Center
            )

            Text(
                "Saha Satış Otomasyonu",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Center
            )

            Spacer(modifier = Modifier.height(32.dp))

            // Switch Tab Pill
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(
                        color = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f),
                        shape = RoundedCornerShape(16.dp)
                    )
                    .padding(4.dp),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                Box(
                    modifier = Modifier
                        .weight(1f)
                        .background(
                            color = if (isLoginTab) MaterialTheme.colorScheme.primary else androidx.compose.ui.graphics.Color.Transparent,
                            shape = RoundedCornerShape(12.dp)
                        )
                        .clickable {
                            isLoginTab = true
                            errorMessage = null
                        }
                        .padding(vertical = 12.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        "Giriş Yap",
                        color = if (isLoginTab) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.bodyMedium
                    )
                }

                Box(
                    modifier = Modifier
                        .weight(1f)
                        .background(
                            color = if (!isLoginTab) MaterialTheme.colorScheme.primary else androidx.compose.ui.graphics.Color.Transparent,
                            shape = RoundedCornerShape(12.dp)
                        )
                        .clickable {
                            isLoginTab = false
                            errorMessage = null
                        }
                        .padding(vertical = 12.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        "Kayıt Ol",
                        color = if (!isLoginTab) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurfaceVariant,
                        fontWeight = FontWeight.Bold,
                        style = MaterialTheme.typography.bodyMedium
                    )
                }
            }

            Spacer(modifier = Modifier.height(24.dp))

            // Display errors if any
            AnimatedVisibility(
                visible = errorMessage != null,
                enter = fadeIn() + expandVertically(),
                exit = fadeOut() + shrinkVertically()
            ) {
                errorMessage?.let {
                    Card(
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.errorContainer
                        ),
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 16.dp),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Row(
                            modifier = Modifier.padding(16.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(
                                Icons.Filled.Error,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.error
                            )
                            Spacer(modifier = Modifier.width(12.dp))
                            Text(
                                text = it,
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onErrorContainer
                            )
                        }
                    }
                }
            }

            // Forms Layout
            if (isLoginTab) {
                // LOGIN SCREEN
                Column(modifier = Modifier.fillMaxWidth()) {
                    OutlinedTextField(
                        value = loginUsername,
                        onValueChange = {
                            loginUsername = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Kullanıcı Adı") },
                        leadingIcon = { Icon(Icons.Filled.Person, contentDescription = null) },
                        singleLine = true,
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    OutlinedTextField(
                        value = loginPassword,
                        onValueChange = {
                            loginPassword = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Parola") },
                        leadingIcon = { Icon(Icons.Filled.Lock, contentDescription = null) },
                        singleLine = true,
                        visualTransformation = if (passwordVisible) VisualTransformation.None else PasswordVisualTransformation(),
                        trailingIcon = {
                            IconButton(onClick = { passwordVisible = !passwordVisible }) {
                                Icon(
                                    imageVector = if (passwordVisible) Icons.Filled.Visibility else Icons.Filled.VisibilityOff,
                                    contentDescription = null
                                )
                            }
                        },
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(12.dp))

                    Text(
                        text = "Demo Girişi için: kullanıcı: admin, şifre: admin",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.primary.copy(alpha = 0.7f),
                        modifier = Modifier.fillMaxWidth(),
                        textAlign = TextAlign.Start
                    )

                    Spacer(modifier = Modifier.height(24.dp))

                    FieldPrimaryButton(
                        onClick = {
                            if (loginUsername.isBlank() || loginPassword.isBlank()) {
                                errorMessage = "Lütfen tüm alanları doldurunuz."
                                return@FieldPrimaryButton
                            }
                            coroutineScope.launch {
                                isLoading = true
                                errorMessage = null
                                try {
                                    val db = DatabaseProvider.getDatabase(context)
                                    val user = withContext(Dispatchers.IO) {
                                        db.userDao().getUserByUsername(loginUsername.trim())
                                    }

                                    if (user != null && user.passwordHash == loginPassword) {
                                        withContext(Dispatchers.IO) {
                                            db.userDao().clearSessions()
                                            db.userDao().markLoggedIn(user.username)
                                            if (user.username == "admin") {
                                                AppDataStore.loadDemoDataSync(context)
                                            } else {
                                                AppDataStore.clearAllDataSync(context)
                                            }
                                        }
                                        Toast.makeText(context, "Giriş başarılı! Hoş geldiniz.", Toast.LENGTH_SHORT).show()
                                        navController.navigate("dashboard") {
                                            popUpTo("login") { inclusive = true }
                                        }
                                    } else {
                                        errorMessage = "Kullanıcı adı veya şifre hatalı."
                                    }
                                } catch (e: Exception) {
                                    errorMessage = "Giriş hatası: ${e.message}"
                                } finally {
                                    isLoading = false
                                }
                            }
                        },
                        enabled = !isLoading && loginUsername.isNotBlank() && loginPassword.isNotBlank(),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        if (isLoading) {
                            CircularProgressIndicator(
                                modifier = Modifier.size(24.dp),
                                color = MaterialTheme.colorScheme.onPrimary,
                                strokeWidth = 2.dp
                            )
                        } else {
                            Text("Giriş Yap")
                        }
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    FieldSecondaryButton(
                        onClick = {
                            coroutineScope.launch {
                                isLoading = true
                                errorMessage = null
                                try {
                                    val db = DatabaseProvider.getDatabase(context)
                                    withContext(Dispatchers.IO) {
                                        // Ensure admin user exists in DB
                                        val adminUser = db.userDao().getUserByUsername("admin")
                                        if (adminUser == null) {
                                            db.userDao().insertUser(
                                                UserEntity(
                                                    username = "admin",
                                                    passwordHash = "admin",
                                                    fullName = "Saha Yöneticisi",
                                                    email = "admin@fieldforce.com",
                                                    isLoggedIn = false
                                                )
                                            )
                                        }
                                        db.userDao().clearSessions()
                                        db.userDao().markLoggedIn("admin")
                                        AppDataStore.loadDemoDataSync(context)
                                    }
                                    Toast.makeText(context, "Demo girişi başarılı! Hoş geldiniz.", Toast.LENGTH_SHORT).show()
                                    navController.navigate("dashboard") {
                                        popUpTo("login") { inclusive = true }
                                    }
                                } catch (e: Exception) {
                                    errorMessage = "Demo giriş hatası: ${e.message}"
                                } finally {
                                    isLoading = false
                                }
                            }
                        },
                        enabled = !isLoading,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        if (isLoading) {
                            CircularProgressIndicator(
                                modifier = Modifier.size(24.dp),
                                color = MaterialTheme.colorScheme.primary,
                                strokeWidth = 2.dp
                            )
                        } else {
                            Icon(Icons.Filled.PlayArrow, contentDescription = null, modifier = Modifier.size(18.dp))
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("Hemen Demo Girişi Yap")
                        }
                    }
                }
            } else {
                // REGISTRATION SCREEN
                Column(modifier = Modifier.fillMaxWidth()) {
                    OutlinedTextField(
                        value = regFullName,
                        onValueChange = {
                            regFullName = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Adınız ve Soyadınız") },
                        leadingIcon = { Icon(Icons.Filled.Badge, contentDescription = null) },
                        singleLine = true,
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    OutlinedTextField(
                        value = regUsername,
                        onValueChange = {
                            regUsername = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Kullanıcı Adı veya Sicil No") },
                        leadingIcon = { Icon(Icons.Filled.Person, contentDescription = null) },
                        singleLine = true,
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    OutlinedTextField(
                        value = regEmail,
                        onValueChange = {
                            regEmail = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("E-posta Adresi") },
                        leadingIcon = { Icon(Icons.Filled.Email, contentDescription = null) },
                        singleLine = true,
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    OutlinedTextField(
                        value = regPassword,
                        onValueChange = {
                            regPassword = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Parola") },
                        leadingIcon = { Icon(Icons.Filled.Lock, contentDescription = null) },
                        singleLine = true,
                        visualTransformation = if (regPasswordVisible) VisualTransformation.None else PasswordVisualTransformation(),
                        trailingIcon = {
                            IconButton(onClick = { regPasswordVisible = !regPasswordVisible }) {
                                Icon(
                                    imageVector = if (regPasswordVisible) Icons.Filled.Visibility else Icons.Filled.VisibilityOff,
                                    contentDescription = null
                                )
                            }
                        },
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    OutlinedTextField(
                        value = regPasswordConfirm,
                        onValueChange = {
                            regPasswordConfirm = it
                            errorMessage = null
                        },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Parola Tekrar") },
                        leadingIcon = { Icon(Icons.Filled.LockReset, contentDescription = null) },
                        singleLine = true,
                        visualTransformation = if (regPasswordVisible) VisualTransformation.None else PasswordVisualTransformation(),
                        shape = RoundedCornerShape(12.dp)
                    )

                    Spacer(modifier = Modifier.height(24.dp))

                    FieldPrimaryButton(
                        onClick = {
                            if (regFullName.isBlank() || regUsername.isBlank() || regEmail.isBlank() || regPassword.isBlank()) {
                                errorMessage = "Lütfen tüm alanları eksiksiz doldurunuz."
                                return@FieldPrimaryButton
                            }
                            if (regPassword != regPasswordConfirm) {
                                errorMessage = "Girilen parolalar birbiriyle eşleşmemektedir."
                                return@FieldPrimaryButton
                            }
                            if (regPassword.length < 4) {
                                errorMessage = "Parola güvenliği için en az 4 karakter girilmelidir."
                                return@FieldPrimaryButton
                            }

                            coroutineScope.launch {
                                isLoading = true
                                errorMessage = null
                                try {
                                    val db = DatabaseProvider.getDatabase(context)
                                    // Check if user already exists
                                    val existing = withContext(Dispatchers.IO) {
                                        db.userDao().getUserByUsername(regUsername.trim().lowercase())
                                    }

                                    if (existing != null) {
                                        errorMessage = "Bu kullanıcı adı zaten alınmıştır."
                                        return@launch
                                    }

                                    val newUser = UserEntity(
                                        username = regUsername.trim().lowercase(),
                                        passwordHash = regPassword,
                                        fullName = regFullName.trim(),
                                        email = regEmail.trim(),
                                        isLoggedIn = true // Automatically log in on signup
                                    )

                                    withContext(Dispatchers.IO) {
                                        db.userDao().clearSessions()
                                        db.userDao().insertUser(newUser)
                                        AppDataStore.clearAllDataSync(context)
                                    }

                                    Toast.makeText(context, "Kaydınız başarıyla tamamlandı!", Toast.LENGTH_LONG).show()
                                    navController.navigate("dashboard") {
                                        popUpTo("login") { inclusive = true }
                                    }
                                } catch (e: Exception) {
                                    errorMessage = "Kayıt hatası: ${e.message}"
                                } finally {
                                    isLoading = false
                                }
                            }
                        },
                        enabled = !isLoading && regFullName.isNotBlank() && regUsername.isNotBlank() && regEmail.isNotBlank() && regPassword.isNotBlank() && regPasswordConfirm.isNotBlank(),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        if (isLoading) {
                            CircularProgressIndicator(
                                modifier = Modifier.size(24.dp),
                                color = MaterialTheme.colorScheme.onPrimary,
                                strokeWidth = 2.dp
                            )
                        } else {
                            Text("Hesap Oluştur ve Giriş Yap")
                        }
                    }
                }
            }
        }
    }
}
