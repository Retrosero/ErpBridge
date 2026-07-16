package com.example.util

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

object CryptoUtils {
    private const val PROVIDER = "AndroidKeyStore"
    private const val ALIAS = "FieldOpsCryptoKey"
    private const val TRANSFORMATION = "AES/GCM/NoPadding"

    init {
        try {
            val keyStore = KeyStore.getInstance(PROVIDER).apply { load(null) }
            if (!keyStore.containsAlias(ALIAS)) {
                val keyGenerator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, PROVIDER)
                val spec = KeyGenParameterSpec.Builder(
                    ALIAS,
                    KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT
                )
                    .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                    .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                    .build()
                keyGenerator.init(spec)
                keyGenerator.generateKey()
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    private fun getSecretKey(): SecretKey {
        val keyStore = KeyStore.getInstance(PROVIDER).apply { load(null) }
        return (keyStore.getEntry(ALIAS, null) as KeyStore.SecretKeyEntry).secretKey
    }

    fun encrypt(plainText: String): String {
        if (plainText.isEmpty()) return ""
        return try {
            val cipher = Cipher.getInstance(TRANSFORMATION)
            cipher.init(Cipher.ENCRYPT_MODE, getSecretKey())
            val iv = cipher.iv
            val encryptedBytes = cipher.doFinal(plainText.toByteArray(Charsets.UTF_8))
            val combined = ByteArray(iv.size + encryptedBytes.size)
            System.arraycopy(iv, 0, combined, 0, iv.size)
            System.arraycopy(encryptedBytes, 0, combined, iv.size, encryptedBytes.size)
            Base64.encodeToString(combined, Base64.DEFAULT)
        } catch (e: Exception) {
            e.printStackTrace()
            // Fallback to simple Base64 encoding in case of environment limitations
            Base64.encodeToString(plainText.toByteArray(Charsets.UTF_8), Base64.DEFAULT)
        }
    }

    fun decrypt(encryptedText: String): String {
        if (encryptedText.isEmpty()) return ""
        return try {
            val combined = Base64.decode(encryptedText, Base64.DEFAULT)
            val ivSize = 12 // Standard GCM IV size is 12 bytes
            val iv = ByteArray(ivSize)
            System.arraycopy(combined, 0, iv, 0, ivSize)
            val encryptedBytes = ByteArray(combined.size - ivSize)
            System.arraycopy(combined, ivSize, encryptedBytes, 0, encryptedBytes.size)

            val cipher = Cipher.getInstance(TRANSFORMATION)
            val spec = GCMParameterSpec(128, iv)
            cipher.init(Cipher.DECRYPT_MODE, getSecretKey(), spec)
            val decryptedBytes = cipher.doFinal(encryptedBytes)
            String(decryptedBytes, Charsets.UTF_8)
        } catch (e: Exception) {
            e.printStackTrace()
            try {
                // Fallback to decoding Simple Base64
                String(Base64.decode(encryptedText, Base64.DEFAULT), Charsets.UTF_8)
            } catch (ex: Exception) {
                encryptedText
            }
        }
    }
}
