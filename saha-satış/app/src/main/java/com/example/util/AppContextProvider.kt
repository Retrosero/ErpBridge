package com.example.util

import android.annotation.SuppressLint
import android.content.Context

@SuppressLint("StaticFieldLeak")
object AppContextProvider {
    var context: Context? = null
}
