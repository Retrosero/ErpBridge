package com.example.util

object PriceFormulaEvaluator {
    fun evaluate(expression: String, currentPrices: Map<String, String>): Double? {
        var expr = expression.trim().lowercase()
        if (expr.isEmpty()) return null

        // 1. Sort the keys descending by length so we match longer names first ("Saha Bayi" before "Bayi")
        val sortedKeys = currentPrices.keys.sortedByDescending { it.length }

        // Try replacing each price type reference
        for (key in sortedKeys) {
            val keyLower = key.trim().lowercase()
            // Generate common variations of Turkish price titles
            val variations = listOf(
                keyLower,
                keyLower.replace("ı", "i").replace("ş", "s").replace("ğ", "g").replace("ö", "o").replace("ü", "u").replace("ç", "c"),
                keyLower + " fiyatı",
                keyLower + " fiyati",
                keyLower + " fiyat"
            )

            for (v in variations) {
                if (expr.contains(v)) {
                    val refValueStr = currentPrices[key] ?: ""
                    // Avoid infinite mutual recursion by passing a filtered model without the current key
                    val refValue = refValueStr.toDoubleOrNull() 
                        ?: evaluate(refValueStr, currentPrices.filterKeys { it != key }) 
                        ?: 0.0
                    expr = expr.replace(v, refValue.toString())
                }
            }
        }

        // 2. Map other common operators
        expr = expr.replace("x", "*")
        expr = expr.replace(" ","") // remove spacing

        // 3. Evaluate mathematical parsed simple expressions
        return try {
            evalSimpleExpression(expr)
        } catch (e: Exception) {
            null
        }
    }

    private fun evalSimpleExpression(expr: String): Double {
        return object {
            var pos = -1
            var ch = 0

            fun nextChar() {
                ch = if (++pos < expr.length) expr[pos].code else -1
            }

            fun eat(charToEat: Int): Boolean {
                while (ch == ' '.code) nextChar()
                if (ch == charToEat) {
                    nextChar()
                    return true
                }
                return false
            }

            fun parse(): Double {
                nextChar()
                val x = parseExpression()
                if (pos < expr.length) throw RuntimeException("Unexpected character: " + ch.toChar())
                return x
            }

            fun parseExpression(): Double {
                var x = parseTerm()
                while (true) {
                    if (eat('+'.code)) x += parseTerm()
                    else if (eat('-'.code)) x -= parseTerm()
                    else return x
                }
            }

            fun parseTerm(): Double {
                var x = parseFactor()
                while (true) {
                    if (eat('*'.code)) x *= parseFactor()
                    else if (eat('/'.code)) x /= parseFactor()
                    else return x
                }
            }

            fun parseFactor(): Double {
                if (eat('+'.code)) return parseFactor()
                if (eat('-'.code)) return -parseFactor()

                var x: Double
                val startPos = pos
                if (eat('('.code)) {
                    x = parseExpression()
                    eat(')'.code)
                } else if ((ch >= '0'.code && ch <= '9'.code) || ch == '.'.code) {
                    while ((ch >= '0'.code && ch <= '9'.code) || ch == '.'.code) nextChar()
                    x = expr.substring(startPos, pos).toDouble()
                } else {
                    throw RuntimeException("Unexpected: " + ch.toChar())
                }
                return x
            }
        }.parse()
    }
}
