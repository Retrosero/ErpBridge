package com.example.ui.screens;

import android.content.Context;
import android.content.SharedPreferences;
import androidx.compose.runtime.MutableState;
import androidx.compose.runtime.SnapshotMutationPolicy;
import androidx.compose.runtime.SnapshotStateKt;
import androidx.compose.runtime.internal.StabilityInferred;
import kotlin.Metadata;
import kotlin.Unit;
import kotlin.coroutines.Continuation;
import kotlin.coroutines.intrinsics.IntrinsicsKt;
import kotlin.jvm.functions.Function1;
import kotlin.jvm.internal.Intrinsics;
import kotlin.jvm.internal.SourceDebugExtension;
import kotlinx.coroutines.BuildersKt;
import kotlinx.coroutines.Dispatchers;
import okhttp3.ResponseBody;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import org.json.JSONObject;
import retrofit2.Response;
/* compiled from: BridgeSyncHelper.kt */
@StabilityInferred(parameters = 1)
@Metadata(d1 = {"\u0000H\n\u0002\u0018\u0002\n\u0002\u0010\u0000\n\u0002\b\u0003\n\u0002\u0018\u0002\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0000\n\u0002\u0018\u0002\n\u0002\u0010\u000e\n\u0002\u0010\u0002\n\u0002\b\u0002\n\u0002\u0018\u0002\n\u0002\b\u0003\n\u0002\u0010\u0007\n\u0002\b\u000e\n\u0002\u0018\u0002\n\u0002\u0010\u000b\n\u0002\b\u0014\bÇ\u0002\u0018\u00002\u00020\u0001B\t\b\u0002¢\u0006\u0004\b\u0002\u0010\u0003J,\u0010\u0004\u001a\u00060\u0005j\u0002`\u00062\n\u0010\u0007\u001a\u0006\u0012\u0002\b\u00030\b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\nH\u0002JN\u0010\r\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u0015\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u0016\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u0017\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u0018\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u0019\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001a\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001b\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001c\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001d\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001e\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010\u001f\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014JN\u0010 \u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\u0012\u0010\t\u001a\u000e\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f0\n2\u0012\u0010\u0012\u001a\u000e\u0012\u0004\u0012\u00020\u0013\u0012\u0004\u0012\u00020\f0\nH\u0086@¢\u0006\u0002\u0010\u0014J\u000e\u0010'\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000fJ\u000e\u0010(\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000fJ\u0016\u0010)\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010*\u001a\u00020#J\u0018\u0010+\u001a\u0004\u0018\u00010\u000b2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010,\u001a\u00020\u000bJ\u001e\u0010-\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010,\u001a\u00020\u000b2\u0006\u0010.\u001a\u00020\u000bJ\u000e\u0010/\u001a\u00020#2\u0006\u0010\u000e\u001a\u00020\u000fJ.\u00100\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0016\b\u0002\u00101\u001a\u0010\u0012\u0004\u0012\u00020\u000b\u0012\u0004\u0012\u00020\f\u0018\u00010\nH\u0086@¢\u0006\u0002\u00102J0\u00103\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\b\u00104\u001a\u0004\u0018\u00010\u000bH\u0086@¢\u0006\u0002\u00105J0\u00106\u001a\u00020\f2\u0006\u0010\u000e\u001a\u00020\u000f2\u0006\u0010\u0010\u001a\u00020\u000b2\u0006\u0010\u0011\u001a\u00020\u000b2\b\u00104\u001a\u0004\u0018\u00010\u000bH\u0086@¢\u0006\u0002\u00105R\u0017\u0010!\u001a\b\u0012\u0004\u0012\u00020#0\"¢\u0006\b\n\u0000\u001a\u0004\b!\u0010$R\u0017\u0010%\u001a\b\u0012\u0004\u0012\u00020\u000b0\"¢\u0006\b\n\u0000\u001a\u0004\b&\u0010$¨\u00067"}, d2 = {"Lcom/example/ui/screens/BridgeSyncHelper;", "", "<init>", "()V", "handleApiError", "Ljava/lang/Exception;", "Lkotlin/Exception;", "response", "Lretrofit2/Response;", "log", "Lkotlin/Function1;", "", "", "syncCariler", "context", "Landroid/content/Context;", "apiUrl", "apiKey", "updateProgress", "", "(Landroid/content/Context;Ljava/lang/String;Ljava/lang/String;Lkotlin/jvm/functions/Function1;Lkotlin/jvm/functions/Function1;Lkotlin/coroutines/Continuation;)Ljava/lang/Object;", "syncUrunler", "syncFiyatListeleri", "syncStokSeviyeleri", "syncFiyatListesiNew", "syncCariHareketleri", "syncFaturaHareket", "syncStatusCheck", "syncCariAdresleri", "syncCariBankaHesaplari", "syncBankalar", "syncKasalar", "syncKasaYonetim", "isOnlineState", "Landroidx/compose/runtime/MutableState;", "", "()Landroidx/compose/runtime/MutableState;", "lastSyncTimeState", "getLastSyncTimeState", "initLastSyncTime", "initOnlineStatus", "setOnlineStatus", "online", "getLastSyncTime", "entity", "setLastSyncTime", "time", "isErpModeActive", "triggerBackgroundSync", "logCallback", "(Landroid/content/Context;Lkotlin/jvm/functions/Function1;Lkotlin/coroutines/Continuation;)Ljava/lang/Object;", "syncCarilerIncremental", "since", "(Landroid/content/Context;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Lkotlin/coroutines/Continuation;)Ljava/lang/Object;", "syncUrunlerIncremental", "app"}, k = 1, mv = {2, 2, 0}, xi = 48)
@SourceDebugExtension({"SMAP\nBridgeSyncHelper.kt\nKotlin\n*S Kotlin\n*F\n+ 1 BridgeSyncHelper.kt\ncom/example/ui/screens/BridgeSyncHelper\n+ 2 Maps.kt\nkotlin/collections/MapsKt__MapsKt\n+ 3 fake.kt\nkotlin/jvm/internal/FakeKt\n+ 4 _Collections.kt\nkotlin/collections/CollectionsKt___CollectionsKt\n*L\n1#1,2226:1\n382#2,7:2227\n1#3:2234\n774#4:2235\n865#4,2:2236\n1563#4:2238\n1634#4,3:2239\n774#4:2242\n865#4,2:2243\n1878#4,3:2245\n1869#4,2:2248\n1563#4:2250\n1634#4,3:2251\n*S KotlinDebug\n*F\n+ 1 BridgeSyncHelper.kt\ncom/example/ui/screens/BridgeSyncHelper\n*L\n634#1:2227,7\n720#1:2235\n720#1:2236,2\n1378#1:2238\n1378#1:2239,3\n1378#1:2242\n1378#1:2243,2\n1434#1:2245,3\n1628#1:2248,2\n2195#1:2250\n2195#1:2251,3\n*E\n"})
/* loaded from: /app/applet/app/build/intermediates/project_dex_archive/debug/dexBuilderDebug/out/com/example/ui/screens/BridgeSyncHelper.dex */
public final class BridgeSyncHelper {
    public static final int $stable = 0;
    @NotNull
    public static final BridgeSyncHelper INSTANCE = new BridgeSyncHelper();
    @NotNull
    private static final MutableState<Boolean> isOnlineState = SnapshotStateKt.mutableStateOf$default(true, (SnapshotMutationPolicy) null, 2, (Object) null);
    @NotNull
    private static final MutableState<String> lastSyncTimeState = SnapshotStateKt.mutableStateOf$default("Henüz Yapılmadı", (SnapshotMutationPolicy) null, 2, (Object) null);

    private BridgeSyncHelper() {
    }

    private final Exception handleApiError(Response<?> response, Function1<? super String, Unit> function1) {
        String userFriendlyMessage;
        String str;
        int code = response.code();
        ResponseBody errorBody = response.errorBody();
        String errorBody2 = (errorBody == null || (errorBody2 = errorBody.string()) == null) ? "" : "";
        String safeMessage = "Bilinmeyen Hata";
        boolean z = true;
        try {
            if (errorBody2.length() > 0) {
                JSONObject json = new JSONObject(errorBody2);
                String msg = json.optString("message", json.optString("error", "Bilinmeyen API Hatası"));
                String errCode = json.optString("code", "");
                Intrinsics.checkNotNull(errCode);
                if (errCode.length() > 0) {
                    str = errCode + " - " + msg;
                } else {
                    Intrinsics.checkNotNull(msg);
                    str = msg;
                }
                safeMessage = str;
            }
        } catch (Exception e) {
            safeMessage = "Yanıt okunamadı";
        }
        if (code == 401 || code == 403) {
            userFriendlyMessage = "Yetkilendirme Hatası: API Anahtarı veya Tenant ID geçersiz (" + safeMessage + ")";
        } else if (code == 422) {
            userFriendlyMessage = "Doğrulama Hatası: Gönderilen parametreler hatalı (" + safeMessage + ")";
        } else if (code == 429) {
            userFriendlyMessage = "İstek Sınırı Aşıldı: Çok fazla istek gönderdiniz (" + safeMessage + ")";
        } else {
            if (500 > code || code >= 600) {
                z = false;
            }
            userFriendlyMessage = z ? "Sunucu Hatası: GoApp Cloud sunucusunda bir sorun oluştu (" + safeMessage + ")" : "Ağ Hatası [" + code + "] (" + safeMessage + ")";
        }
        return new Exception(userFriendlyMessage);
    }

    /* JADX WARN: Can't wrap try/catch for region: R(9:115|(2:78|79)|(2:251|252)(4:83|(4:85|86|87|88)(9:125|126|127|128|(26:131|132|(21:140|141|(1:143)|144|(3:187|188|(16:190|(13:193|(4:195|196|197|198)(1:220)|199|(1:201)|202|(1:204)|216|206|(1:208)(1:215)|209|(2:211|212)(1:214)|213|191)|221|222|223|155|156|(1:158)(1:177)|159|160|(1:162)(1:174)|163|(1:165)(1:173)|166|(2:168|169)(2:171|172)|170))|146|(1:148)(1:186)|149|(4:179|180|181|182)(2:151|(1:153)(1:178))|154|155|156|(0)(0)|159|160|(0)(0)|163|(0)(0)|166|(0)(0)|170)|227|228|141|(0)|144|(0)|146|(0)(0)|149|(0)(0)|154|155|156|(0)(0)|159|160|(0)(0)|163|(0)(0)|166|(0)(0)|170|129)|232|233|(4:235|(1:237)(1:246)|(3:239|(1:241)(1:244)|(1:243))|245)|247)|89|(3:91|92|(2:94|(1:96)(11:97|59|60|61|62|63|64|35|36|19|20))(2:98|(1:100)(9:101|31|32|33|34|35|36|19|20)))(0))|175|176|40|41|42|(1:44)(7:45|15|16|17|18|19|20)) */
    /* JADX WARN: Code restructure failed: missing block: B:107:0x0399, code lost:
        if (r4 == null) goto L216;
     */
    /* JADX WARN: Code restructure failed: missing block: B:230:0x0b10, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:231:0x0b11, code lost:
        r7 = r9;
        r9 = r11;
     */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0038  */
    /* JADX WARN: Removed duplicated region for block: B:125:0x0417  */
    /* JADX WARN: Removed duplicated region for block: B:126:0x041a  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0048  */
    /* JADX WARN: Removed duplicated region for block: B:134:0x04b8  */
    /* JADX WARN: Removed duplicated region for block: B:142:0x05cc  */
    /* JADX WARN: Removed duplicated region for block: B:143:0x05cf  */
    /* JADX WARN: Removed duplicated region for block: B:146:0x05d7  */
    /* JADX WARN: Removed duplicated region for block: B:147:0x05da  */
    /* JADX WARN: Removed duplicated region for block: B:150:0x05e2  */
    /* JADX WARN: Removed duplicated region for block: B:151:0x05e5  */
    /* JADX WARN: Removed duplicated region for block: B:154:0x05ed  */
    /* JADX WARN: Removed duplicated region for block: B:155:0x05f0  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x0075  */
    /* JADX WARN: Removed duplicated region for block: B:183:0x06e0  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00bc  */
    /* JADX WARN: Removed duplicated region for block: B:255:0x01cb A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:257:0x0333 A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:279:0x0427 A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:27:0x00fd  */
    /* JADX WARN: Removed duplicated region for block: B:32:0x0178  */
    /* JADX WARN: Removed duplicated region for block: B:87:0x0329 A[Catch: Exception -> 0x02f8, TRY_ENTER, TRY_LEAVE, TryCatch #8 {Exception -> 0x02f8, blocks: (B:74:0x02f3, B:87:0x0329), top: B:251:0x02f3 }] */
    /* JADX WARN: Type inference failed for: r2v75, types: [java.util.List] */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:54:0x0269 -> B:281:0x0280). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncCariler(@org.jetbrains.annotations.NotNull android.content.Context r75, @org.jetbrains.annotations.NotNull java.lang.String r76, @org.jetbrains.annotations.NotNull java.lang.String r77, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r78, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r79, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r80) {
        /*
            Method dump skipped, instructions count: 2888
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncCariler(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Can't wrap try/catch for region: R(36:658|(4:659|660|661|(6:662|663|664|665|666|667))|(27:518|519|520|(1:522)|523|(1:525)(1:598)|526|527|528|529|530|531|532|533|534|535|536|537|538|539|540|541|542|543|544|545|(1:547)(6:548|505|506|(1:651)(2:510|(4:512|513|514|515)(5:618|(5:621|(1:645)(1:625)|(8:631|632|633|634|635|636|637|639)(3:627|628|629)|630|619)|646|647|(1:649)(1:650)))|516|(30:602|603|604|605|606|607|608|562|563|564|565|566|567|(30:355|356|357|(1:359)|360|(1:362)(1:428)|363|364|365|366|367|368|369|370|371|372|373|374|375|376|377|378|379|380|381|382|383|384|385|(1:387)(6:388|342|343|(1:494)(2:347|(4:349|350|351|352)(5:442|(7:445|(1:488)(1:449)|(1:487)(3:451|(1:486)(1:455)|(9:461|462|463|464|465|(4:467|468|469|470)(1:482)|471|(3:476|477|478)(3:473|474|475)|460)(1:457))|458|459|460|443)|489|490|(1:492)(1:493)))|353|(11:432|433|434|435|(12:86|87|88|(1:90)|91|(1:93)(1:107)|94|95|96|97|98|(1:100)(10:101|73|74|(2:332|333)(4:78|(4:80|81|82|83)(9:144|145|146|147|(42:150|151|152|153|(3:155|156|157)(1:309)|158|(2:159|(2:161|(1:164)(1:163))(2:304|305))|165|(1:303)(1:171)|(1:173)(1:(1:289)(3:290|(1:302)|292))|174|175|(3:272|273|(2:279|(3:285|286|287)(2:283|284))(2:277|278))(4:179|(1:181)|182|183)|184|(2:265|266)|186|187|(23:192|(2:194|(1:196)(1:262))(1:263)|197|198|(6:201|(2:203|(2:205|(5:207|208|209|210|(3:212|(2:214|215)(2:217|218)|216))(1:222))(1:223))(1:224)|219|(0)(0)|216|199)|225|226|(1:228)(1:259)|229|(1:231)(1:258)|232|(1:234)(1:257)|235|(1:237)|238|(1:240)|241|(1:243)|244|(1:246)(1:256)|247|(2:251|252)|253)|264|(0)(0)|197|198|(1:199)|225|226|(0)(0)|229|(0)(0)|232|(0)(0)|235|(0)|238|(0)|241|(0)|244|(0)(0)|247|(1:255)(3:249|251|252)|253|148)|313|314|(4:316|(1:318)(1:327)|(3:320|(1:322)(1:325)|(1:324))|326)|328)|84|(4:111|112|113|(3:132|133|(1:135)(14:136|57|58|59|60|61|62|63|35|36|37|38|20|21))(8:115|116|117|118|119|120|121|(1:123)(11:124|31|32|33|34|35|36|37|38|20|21)))(0))|220|221|42|43|44|(1:46)(7:47|16|17|18|19|20|21)))(0)|105|106|42|43|44|(0)(0))(0)))(0)|392|393|394|395|396|397|398|399|400|(0)(0)|105|106|42|43|44|(0)(0))(0)))(0)|552|553|554|555|556|557|558|559|560|561|562|563|564|565|566|567|(0)(0)|392|393|394|395|396|397|398|399|400|(0)(0)|105|106|42|43|44|(0)(0)) */
    /* JADX WARN: Can't wrap try/catch for region: R(44:658|659|660|661|662|663|664|665|666|667|(27:518|519|520|(1:522)|523|(1:525)(1:598)|526|527|528|529|530|531|532|533|534|535|536|537|538|539|540|541|542|543|544|545|(1:547)(6:548|505|506|(1:651)(2:510|(4:512|513|514|515)(5:618|(5:621|(1:645)(1:625)|(8:631|632|633|634|635|636|637|639)(3:627|628|629)|630|619)|646|647|(1:649)(1:650)))|516|(30:602|603|604|605|606|607|608|562|563|564|565|566|567|(30:355|356|357|(1:359)|360|(1:362)(1:428)|363|364|365|366|367|368|369|370|371|372|373|374|375|376|377|378|379|380|381|382|383|384|385|(1:387)(6:388|342|343|(1:494)(2:347|(4:349|350|351|352)(5:442|(7:445|(1:488)(1:449)|(1:487)(3:451|(1:486)(1:455)|(9:461|462|463|464|465|(4:467|468|469|470)(1:482)|471|(3:476|477|478)(3:473|474|475)|460)(1:457))|458|459|460|443)|489|490|(1:492)(1:493)))|353|(11:432|433|434|435|(12:86|87|88|(1:90)|91|(1:93)(1:107)|94|95|96|97|98|(1:100)(10:101|73|74|(2:332|333)(4:78|(4:80|81|82|83)(9:144|145|146|147|(42:150|151|152|153|(3:155|156|157)(1:309)|158|(2:159|(2:161|(1:164)(1:163))(2:304|305))|165|(1:303)(1:171)|(1:173)(1:(1:289)(3:290|(1:302)|292))|174|175|(3:272|273|(2:279|(3:285|286|287)(2:283|284))(2:277|278))(4:179|(1:181)|182|183)|184|(2:265|266)|186|187|(23:192|(2:194|(1:196)(1:262))(1:263)|197|198|(6:201|(2:203|(2:205|(5:207|208|209|210|(3:212|(2:214|215)(2:217|218)|216))(1:222))(1:223))(1:224)|219|(0)(0)|216|199)|225|226|(1:228)(1:259)|229|(1:231)(1:258)|232|(1:234)(1:257)|235|(1:237)|238|(1:240)|241|(1:243)|244|(1:246)(1:256)|247|(2:251|252)|253)|264|(0)(0)|197|198|(1:199)|225|226|(0)(0)|229|(0)(0)|232|(0)(0)|235|(0)|238|(0)|241|(0)|244|(0)(0)|247|(1:255)(3:249|251|252)|253|148)|313|314|(4:316|(1:318)(1:327)|(3:320|(1:322)(1:325)|(1:324))|326)|328)|84|(4:111|112|113|(3:132|133|(1:135)(14:136|57|58|59|60|61|62|63|35|36|37|38|20|21))(8:115|116|117|118|119|120|121|(1:123)(11:124|31|32|33|34|35|36|37|38|20|21)))(0))|220|221|42|43|44|(1:46)(7:47|16|17|18|19|20|21)))(0)|105|106|42|43|44|(0)(0))(0)))(0)|392|393|394|395|396|397|398|399|400|(0)(0)|105|106|42|43|44|(0)(0))(0)))(0)|552|553|554|555|556|557|558|559|560|561|562|563|564|565|566|567|(0)(0)|392|393|394|395|396|397|398|399|400|(0)(0)|105|106|42|43|44|(0)(0)) */
    /* JADX WARN: Can't wrap try/catch for region: R(7:1|(2:3|(4:5|6|7|8))|679|6|7|8|(1:(0))) */
    /* JADX WARN: Code restructure failed: missing block: B:250:0x0a2e, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:251:0x0a2f, code lost:
        r1 = r7;
        r45 = r10;
        r44 = r15;
        r10 = r33;
        r33 = r34;
        r7 = r5;
        r5 = r12;
        r34 = r30;
        r30 = r32;
        r32 = r40;
        r12 = r6;
        r6 = r0;
     */
    /* JADX WARN: Code restructure failed: missing block: B:26:0x01ca, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:27:0x01cb, code lost:
        r2 = r81;
        r1 = r2;
        r8 = "Merkez Depo";
        r10 = r5;
        r31 = r6;
        r14 = "Soğuk Hava Depo";
        r13 = "1";
        r6 = r84;
        r12 = r4;
        r4 = r83;
     */
    /* JADX WARN: Code restructure failed: missing block: B:364:0x0d10, code lost:
        if (r5 == null) goto L186;
     */
    /* JADX WARN: Code restructure failed: missing block: B:512:0x132b, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:513:0x132c, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r2 = r81;
        r10 = r5;
        r6 = r12;
        r1 = r33;
        r12 = r7;
        r7 = r80;
     */
    /* JADX WARN: Code restructure failed: missing block: B:514:0x133e, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:516:0x1340, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:517:0x1341, code lost:
        r80 = r1;
     */
    /* JADX WARN: Code restructure failed: missing block: B:518:0x1343, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r10 = r5;
        r6 = r12;
        r1 = r33;
        r12 = r7;
        r7 = r80;
     */
    /* JADX WARN: Code restructure failed: missing block: B:519:0x1355, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:520:0x1356, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r10 = r12;
        r1 = r34;
        r12 = r5;
     */
    /* JADX WARN: Code restructure failed: missing block: B:521:0x1363, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:522:0x1364, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r2 = r80;
        r10 = r12;
        r1 = r34;
        r12 = r82;
     */
    /* JADX WARN: Code restructure failed: missing block: B:523:0x1375, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:524:0x1376, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r2 = r80;
        r10 = r12;
        r1 = r34;
        r12 = r5;
     */
    /* JADX WARN: Code restructure failed: missing block: B:525:0x1388, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:526:0x1389, code lost:
        r13 = r36;
        r14 = r37;
        r8 = r38;
        r2 = r80;
        r10 = r12;
        r1 = r34;
        r12 = r5;
     */
    /* JADX WARN: Code restructure failed: missing block: B:540:0x15b5, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:541:0x15b6, code lost:
        r8 = r1;
        r1 = r2;
        r2 = r4;
        r4 = r6;
        r7 = r12;
     */
    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0056  */
    /* JADX WARN: Removed duplicated region for block: B:124:0x065a  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x005e  */
    /* JADX WARN: Removed duplicated region for block: B:147:0x0731  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x008e  */
    /* JADX WARN: Removed duplicated region for block: B:20:0x00e7  */
    /* JADX WARN: Removed duplicated region for block: B:23:0x013c  */
    /* JADX WARN: Removed duplicated region for block: B:245:0x09e7  */
    /* JADX WARN: Removed duplicated region for block: B:259:0x0a78  */
    /* JADX WARN: Removed duplicated region for block: B:28:0x01df  */
    /* JADX WARN: Removed duplicated region for block: B:33:0x02a8  */
    /* JADX WARN: Removed duplicated region for block: B:377:0x0d49 A[Catch: Exception -> 0x0d13, TryCatch #19 {Exception -> 0x0d13, blocks: (B:363:0x0d0a, B:371:0x0d3b, B:377:0x0d49, B:379:0x0d55), top: B:584:0x0d0a }] */
    /* JADX WARN: Removed duplicated region for block: B:382:0x0d63  */
    /* JADX WARN: Removed duplicated region for block: B:386:0x0d82 A[Catch: Exception -> 0x0ef4, TryCatch #5 {Exception -> 0x0ef4, blocks: (B:361:0x0d02, B:369:0x0d33, B:383:0x0d67, B:384:0x0d7c, B:386:0x0d82, B:388:0x0d9a, B:390:0x0daa, B:368:0x0d2c, B:360:0x0cff), top: B:556:0x0d67 }] */
    /* JADX WARN: Removed duplicated region for block: B:38:0x035e  */
    /* JADX WARN: Removed duplicated region for block: B:402:0x0de4 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:403:0x0dee  */
    /* JADX WARN: Removed duplicated region for block: B:407:0x0e19 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:408:0x0e26 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:411:0x0e3d A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:412:0x0e42 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:415:0x0e51 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:416:0x0e56 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:419:0x0e65 A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:422:0x0e7c  */
    /* JADX WARN: Removed duplicated region for block: B:425:0x0e86  */
    /* JADX WARN: Removed duplicated region for block: B:428:0x0eaa A[Catch: Exception -> 0x0fd2, TryCatch #51 {Exception -> 0x0fd2, blocks: (B:394:0x0dbf, B:402:0x0de4, B:405:0x0dfe, B:407:0x0e19, B:409:0x0e33, B:411:0x0e3d, B:413:0x0e49, B:415:0x0e51, B:417:0x0e5d, B:419:0x0e65, B:420:0x0e69, B:423:0x0e7e, B:426:0x0e88, B:428:0x0eaa, B:430:0x0eb2, B:432:0x0ec3, B:435:0x0ecb, B:416:0x0e56, B:412:0x0e42, B:408:0x0e26, B:440:0x0f2c, B:442:0x0f3a, B:444:0x0f40, B:447:0x0f48, B:449:0x0f4e, B:453:0x0f57, B:454:0x0f5e, B:458:0x0fa0, B:459:0x0fd1), top: B:647:0x0dbf }] */
    /* JADX WARN: Removed duplicated region for block: B:429:0x0eb0  */
    /* JADX WARN: Removed duplicated region for block: B:468:0x1028  */
    /* JADX WARN: Removed duplicated region for block: B:46:0x03e6  */
    /* JADX WARN: Removed duplicated region for block: B:534:0x159b A[RETURN] */
    /* JADX WARN: Removed duplicated region for block: B:535:0x159c  */
    /* JADX WARN: Type inference failed for: r12v2, types: [java.lang.Object] */
    /* JADX WARN: Type inference failed for: r12v46 */
    /* JADX WARN: Type inference failed for: r4v112, types: [java.util.Map] */
    /* JADX WARN: Type inference failed for: r4v114, types: [java.util.List] */
    /* JADX WARN: Type inference failed for: r4v68, types: [java.util.Map] */
    /* JADX WARN: Type inference failed for: r4v70, types: [java.util.List] */
    /* JADX WARN: Type inference failed for: r80v0, types: [android.content.Context] */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:173:0x07d8 -> B:661:0x07f4). Please submit an issue!!! */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:273:0x0b28 -> B:548:0x0b45). Please submit an issue!!! */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:70:0x0487 -> B:614:0x04a5). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncUrunler(@org.jetbrains.annotations.NotNull android.content.Context r80, @org.jetbrains.annotations.NotNull java.lang.String r81, @org.jetbrains.annotations.NotNull java.lang.String r82, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r83, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r84, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r85) {
        /*
            Method dump skipped, instructions count: 5626
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncUrunler(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Can't wrap try/catch for region: R(7:1|(2:3|(4:5|6|7|8))|111|6|7|8|(1:(0))) */
    /* JADX WARN: Can't wrap try/catch for region: R(8:89|(4:28|29|(1:71)(3:33|(1:70)|37)|38)|(3:52|53|(1:55)(6:56|57|58|59|60|(1:62)(5:63|15|16|17|18)))(1:40)|41|42|43|44|(1:46)(5:47|15|16|17|18)) */
    /* JADX WARN: Code restructure failed: missing block: B:24:0x01a3, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:94:0x050c, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:95:0x050d, code lost:
        r4 = r27;
        r5 = r14;
     */
    /* JADX WARN: Removed duplicated region for block: B:103:0x03d0 A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:10:0x003b  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0043  */
    /* JADX WARN: Removed duplicated region for block: B:15:0x00a3  */
    /* JADX WARN: Removed duplicated region for block: B:18:0x0103  */
    /* JADX WARN: Removed duplicated region for block: B:21:0x015e  */
    /* JADX WARN: Removed duplicated region for block: B:26:0x01a8  */
    /* JADX WARN: Removed duplicated region for block: B:52:0x034b A[RETURN] */
    /* JADX WARN: Removed duplicated region for block: B:53:0x034c  */
    /* JADX WARN: Removed duplicated region for block: B:82:0x0479  */
    /* JADX WARN: Removed duplicated region for block: B:87:0x04f0 A[RETURN] */
    /* JADX WARN: Removed duplicated region for block: B:88:0x04f1  */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncFiyatListeleri(@org.jetbrains.annotations.NotNull android.content.Context r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull java.lang.String r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r34, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r35) {
        /*
            Method dump skipped, instructions count: 1366
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncFiyatListeleri(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:101:0x020f A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:108:0x014d A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x0079  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00f3  */
    /* JADX WARN: Removed duplicated region for block: B:68:0x030c  */
    /* JADX WARN: Type inference failed for: r3v31, types: [java.util.List] */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:42:0x01eb -> B:105:0x0207). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncStokSeviyeleri(@org.jetbrains.annotations.NotNull android.content.Context r29, @org.jetbrains.annotations.NotNull java.lang.String r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r34) {
        /*
            Method dump skipped, instructions count: 1002
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncStokSeviyeleri(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /*  JADX ERROR: JadxRuntimeException in pass: BlockProcessor
        jadx.core.utils.exceptions.JadxRuntimeException: Unreachable block: B:53:0x03ae
        	at jadx.core.dex.visitors.blocks.BlockProcessor.checkForUnreachableBlocks(BlockProcessor.java:81)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.processBlocksTree(BlockProcessor.java:47)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.visit(BlockProcessor.java:39)
        */
    @org.jetbrains.annotations.Nullable
    public final java.lang.Object syncFiyatListesiNew(@org.jetbrains.annotations.NotNull android.content.Context r50, @org.jetbrains.annotations.NotNull java.lang.String r51, @org.jetbrains.annotations.NotNull java.lang.String r52, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r53, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r54, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r55) {
        /*
            Method dump skipped, instructions count: 2768
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncFiyatListesiNew(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:120:0x015d A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:125:0x021c A[EXC_TOP_SPLITTER, SYNTHETIC] */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007f  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x0100  */
    /* JADX WARN: Removed duplicated region for block: B:70:0x0326  */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:42:0x01fb -> B:110:0x0214). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncCariHareketleri(@org.jetbrains.annotations.NotNull android.content.Context r32, @org.jetbrains.annotations.NotNull java.lang.String r33, @org.jetbrains.annotations.NotNull java.lang.String r34, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r35, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r36, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r37) {
        /*
            Method dump skipped, instructions count: 1078
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncCariHareketleri(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /*  JADX ERROR: JadxRuntimeException in pass: BlockProcessor
        jadx.core.utils.exceptions.JadxRuntimeException: Unreachable block: B:95:0x06e2
        	at jadx.core.dex.visitors.blocks.BlockProcessor.checkForUnreachableBlocks(BlockProcessor.java:81)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.processBlocksTree(BlockProcessor.java:47)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.visit(BlockProcessor.java:39)
        */
    @org.jetbrains.annotations.Nullable
    public final java.lang.Object syncFaturaHareket(@org.jetbrains.annotations.NotNull android.content.Context r79, @org.jetbrains.annotations.NotNull java.lang.String r80, @org.jetbrains.annotations.NotNull java.lang.String r81, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r82, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r83, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r84) {
        /*
            Method dump skipped, instructions count: 6028
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncFaturaHareket(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0034  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x003c  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007f  */
    /* JADX WARN: Removed duplicated region for block: B:54:0x01cc A[Catch: Exception -> 0x01a3, TRY_ENTER, TRY_LEAVE, TryCatch #8 {Exception -> 0x01a3, blocks: (B:44:0x0191, B:54:0x01cc), top: B:104:0x0191 }] */
    /* JADX WARN: Removed duplicated region for block: B:58:0x01dd A[Catch: Exception -> 0x025b, TRY_LEAVE, TryCatch #2 {Exception -> 0x025b, blocks: (B:42:0x0170, B:52:0x01b0, B:55:0x01d0, B:56:0x01d7, B:58:0x01dd, B:62:0x01fb), top: B:92:0x0170 }] */
    /* JADX WARN: Type inference failed for: r29v2 */
    /* JADX WARN: Type inference failed for: r29v4 */
    /* JADX WARN: Type inference failed for: r29v5 */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncStatusCheck(@org.jetbrains.annotations.NotNull android.content.Context r28, @org.jetbrains.annotations.NotNull java.lang.String r29, @org.jetbrains.annotations.NotNull java.lang.String r30, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r31, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r32, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r33) {
        /*
            Method dump skipped, instructions count: 728
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncStatusCheck(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007e  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00f8  */
    /* JADX WARN: Removed duplicated region for block: B:30:0x014e  */
    /* JADX WARN: Removed duplicated region for block: B:58:0x02c1  */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:41:0x01ea -> B:86:0x0206). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncCariAdresleri(@org.jetbrains.annotations.NotNull android.content.Context r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull java.lang.String r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r34, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r35) {
        /*
            Method dump skipped, instructions count: 914
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncCariAdresleri(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007e  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00f8  */
    /* JADX WARN: Removed duplicated region for block: B:30:0x014e  */
    /* JADX WARN: Removed duplicated region for block: B:58:0x02c1  */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:41:0x01ea -> B:86:0x0206). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncCariBankaHesaplari(@org.jetbrains.annotations.NotNull android.content.Context r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull java.lang.String r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r34, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r35) {
        /*
            Method dump skipped, instructions count: 914
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncCariBankaHesaplari(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007e  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00f8  */
    /* JADX WARN: Removed duplicated region for block: B:30:0x014e  */
    /* JADX WARN: Removed duplicated region for block: B:58:0x02c1  */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:41:0x01ea -> B:86:0x0206). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncBankalar(@org.jetbrains.annotations.NotNull android.content.Context r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull java.lang.String r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r34, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r35) {
        /*
            Method dump skipped, instructions count: 914
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncBankalar(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0032  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x0040  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x007e  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00f8  */
    /* JADX WARN: Removed duplicated region for block: B:30:0x014e  */
    /* JADX WARN: Removed duplicated region for block: B:58:0x02c1  */
    /* JADX WARN: Unsupported multi-entry loop pattern (BACK_EDGE: B:41:0x01ea -> B:86:0x0206). Please submit an issue!!! */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncKasalar(@org.jetbrains.annotations.NotNull android.content.Context r30, @org.jetbrains.annotations.NotNull java.lang.String r31, @org.jetbrains.annotations.NotNull java.lang.String r32, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r33, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r34, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r35) {
        /*
            Method dump skipped, instructions count: 914
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncKasalar(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX WARN: Can't wrap try/catch for region: R(17:1|(2:3|(7:5|6|7|41|42|43|(7:60|61|62|21|22|23|24)(7:47|48|49|50|51|52|(1:54)(11:55|15|16|17|18|19|20|21|22|23|24))))|92|6|7|41|42|43|(1:45)|60|61|62|21|22|23|24|(1:(0))) */
    /* JADX WARN: Code restructure failed: missing block: B:64:0x0275, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:66:0x0277, code lost:
        r0 = e;
     */
    /* JADX WARN: Code restructure failed: missing block: B:68:0x027d, code lost:
        r0 = e;
     */
    /* JADX WARN: Removed duplicated region for block: B:10:0x0034  */
    /* JADX WARN: Removed duplicated region for block: B:12:0x003e  */
    /* JADX WARN: Removed duplicated region for block: B:17:0x0097  */
    /* JADX WARN: Removed duplicated region for block: B:22:0x00dd  */
    @org.jetbrains.annotations.Nullable
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct add '--show-bad-code' argument
    */
    public final java.lang.Object syncKasaYonetim(@org.jetbrains.annotations.NotNull android.content.Context r27, @org.jetbrains.annotations.NotNull java.lang.String r28, @org.jetbrains.annotations.NotNull java.lang.String r29, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.String, kotlin.Unit> r30, @org.jetbrains.annotations.NotNull kotlin.jvm.functions.Function1<? super java.lang.Float, kotlin.Unit> r31, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r32) {
        /*
            Method dump skipped, instructions count: 694
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncKasaYonetim(android.content.Context, java.lang.String, java.lang.String, kotlin.jvm.functions.Function1, kotlin.jvm.functions.Function1, kotlin.coroutines.Continuation):java.lang.Object");
    }

    @NotNull
    public final MutableState<Boolean> isOnlineState() {
        return isOnlineState;
    }

    @NotNull
    public final MutableState<String> getLastSyncTimeState() {
        return lastSyncTimeState;
    }

    public final void initLastSyncTime(@NotNull Context context) {
        Intrinsics.checkNotNullParameter(context, "context");
        SharedPreferences syncPrefs = context.getSharedPreferences("erp_sync_times", 0);
        String lastGlobal = syncPrefs.getString("last_global_sync", "Henüz Yapılmadı");
        lastSyncTimeState.setValue(lastGlobal != null ? lastGlobal : "Henüz Yapılmadı");
    }

    public final void initOnlineStatus(@NotNull Context context) {
        Intrinsics.checkNotNullParameter(context, "context");
        SharedPreferences prefs = context.getSharedPreferences("erp_sync_times", 0);
        isOnlineState.setValue(Boolean.valueOf(prefs.getBoolean("is_online", true)));
    }

    public final void setOnlineStatus(@NotNull Context context, boolean online) {
        Intrinsics.checkNotNullParameter(context, "context");
        isOnlineState.setValue(Boolean.valueOf(online));
        SharedPreferences prefs = context.getSharedPreferences("erp_sync_times", 0);
        prefs.edit().putBoolean("is_online", online).apply();
    }

    @Nullable
    public final String getLastSyncTime(@NotNull Context context, @NotNull String entity) {
        Intrinsics.checkNotNullParameter(context, "context");
        Intrinsics.checkNotNullParameter(entity, "entity");
        SharedPreferences prefs = context.getSharedPreferences("erp_sync_times", 0);
        return prefs.getString("last_sync_" + entity, null);
    }

    public final void setLastSyncTime(@NotNull Context context, @NotNull String entity, @NotNull String time) {
        Intrinsics.checkNotNullParameter(context, "context");
        Intrinsics.checkNotNullParameter(entity, "entity");
        Intrinsics.checkNotNullParameter(time, "time");
        SharedPreferences prefs = context.getSharedPreferences("erp_sync_times", 0);
        prefs.edit().putString("last_sync_" + entity, time).apply();
    }

    public final boolean isErpModeActive(@NotNull Context context) {
        Intrinsics.checkNotNullParameter(context, "context");
        SharedPreferences prefs = context.getApplicationContext().getSharedPreferences("erp_settings", 0);
        return prefs.getBoolean("is_erp_active", true);
    }

    /* JADX WARN: Multi-variable type inference failed */
    public static /* synthetic */ Object triggerBackgroundSync$default(BridgeSyncHelper bridgeSyncHelper, Context context, Function1 function1, Continuation continuation, int i, Object obj) {
        if ((i & 2) != 0) {
            function1 = null;
        }
        return bridgeSyncHelper.triggerBackgroundSync(context, function1, continuation);
    }

    @Nullable
    public final Object triggerBackgroundSync(@NotNull Context context, @Nullable Function1<? super String, Unit> function1, @NotNull Continuation<? super Unit> continuation) {
        Object withContext = BuildersKt.withContext(Dispatchers.getIO(), new triggerBackgroundSync.2(context, function1, (Continuation) null), continuation);
        return withContext == IntrinsicsKt.getCOROUTINE_SUSPENDED() ? withContext : Unit.INSTANCE;
    }

    /*  JADX ERROR: JadxRuntimeException in pass: BlockProcessor
        jadx.core.utils.exceptions.JadxRuntimeException: Unreachable block: B:140:0x03be
        	at jadx.core.dex.visitors.blocks.BlockProcessor.checkForUnreachableBlocks(BlockProcessor.java:81)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.processBlocksTree(BlockProcessor.java:47)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.visit(BlockProcessor.java:39)
        */
    @org.jetbrains.annotations.Nullable
    public final java.lang.Object syncCarilerIncremental(@org.jetbrains.annotations.NotNull android.content.Context r51, @org.jetbrains.annotations.NotNull java.lang.String r52, @org.jetbrains.annotations.NotNull java.lang.String r53, @org.jetbrains.annotations.Nullable java.lang.String r54, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r55) {
        /*
            Method dump skipped, instructions count: 1166
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncCarilerIncremental(android.content.Context, java.lang.String, java.lang.String, java.lang.String, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX INFO: Access modifiers changed from: package-private */
    public static final Unit syncCarilerIncremental$lambda$11(String it) {
        Intrinsics.checkNotNullParameter(it, "it");
        return Unit.INSTANCE;
    }

    /*  JADX ERROR: JadxRuntimeException in pass: BlockProcessor
        jadx.core.utils.exceptions.JadxRuntimeException: Unreachable block: B:124:0x039c
        	at jadx.core.dex.visitors.blocks.BlockProcessor.checkForUnreachableBlocks(BlockProcessor.java:81)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.processBlocksTree(BlockProcessor.java:47)
        	at jadx.core.dex.visitors.blocks.BlockProcessor.visit(BlockProcessor.java:39)
        */
    @org.jetbrains.annotations.Nullable
    public final java.lang.Object syncUrunlerIncremental(@org.jetbrains.annotations.NotNull android.content.Context r57, @org.jetbrains.annotations.NotNull java.lang.String r58, @org.jetbrains.annotations.NotNull java.lang.String r59, @org.jetbrains.annotations.Nullable java.lang.String r60, @org.jetbrains.annotations.NotNull kotlin.coroutines.Continuation<? super kotlin.Unit> r61) {
        /*
            Method dump skipped, instructions count: 1268
            To view this dump add '--comments-level debug' option
        */
        throw new UnsupportedOperationException("Method not decompiled: com.example.ui.screens.BridgeSyncHelper.syncUrunlerIncremental(android.content.Context, java.lang.String, java.lang.String, java.lang.String, kotlin.coroutines.Continuation):java.lang.Object");
    }

    /* JADX INFO: Access modifiers changed from: package-private */
    public static final Unit syncUrunlerIncremental$lambda$12(String it) {
        Intrinsics.checkNotNullParameter(it, "it");
        return Unit.INSTANCE;
    }
}
