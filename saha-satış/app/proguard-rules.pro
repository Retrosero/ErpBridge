# Keep Kotlin metadata for Moshi
-keep class kotlin.Metadata { *; }

# Keep Moshi's internal types
-keep class com.squareup.moshi.** { *; }
-keep interface com.squareup.moshi.** { *; }

# Keep all classes with @Keep
-keep @androidx.annotation.Keep class * {
    *;
}

# Add standard Room rules just in case
-keep class * extends androidx.room.RoomDatabase {
    *;
}
