.class public final Lcom/kamcord/android/KC_c;
.super Lcom/kamcord/android/KC_ad;


# static fields
.field private static b:I

.field private static c:I


# direct methods
.method static constructor <clinit>()V
    .locals 1

    const/4 v0, 0x5

    sput v0, Lcom/kamcord/android/KC_c;->b:I

    const/16 v0, 0x4e20

    sput v0, Lcom/kamcord/android/KC_c;->c:I

    return-void
.end method

.method public constructor <init>()V
    .locals 1

    const-string v0, "kamcord-analytics"

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_ad;-><init>(Ljava/lang/String;)V

    return-void
.end method

.method public static a(I)V
    .locals 0

    invoke-static {p0}, Lcom/kamcord/android/KC_m;->b(I)V

    return-void
.end method

.method private static a(Lorg/json/JSONArray;Lorg/json/JSONObject;)V
    .locals 5
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Lorg/json/JSONException;
        }
    .end annotation

    const-string v0, "name"

    invoke-virtual {p1, v0}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "grouping_id"

    invoke-virtual {p1, v0}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "bins"

    invoke-virtual {p1, v0}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "bins"

    invoke-virtual {p1, v0}, Lorg/json/JSONObject;->getJSONArray(Ljava/lang/String;)Lorg/json/JSONArray;

    move-result-object v1

    const/4 v0, 0x0

    :goto_0
    invoke-virtual {v1}, Lorg/json/JSONArray;->length()I

    move-result v2

    if-ge v0, v2, :cond_0

    invoke-virtual {v1, v0}, Lorg/json/JSONArray;->getJSONObject(I)Lorg/json/JSONObject;

    move-result-object v2

    const-string v3, "name"

    const-string v4, "name"

    invoke-virtual {p1, v4}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v3, "grouping_id"

    const-string v4, "grouping_id"

    invoke-virtual {p1, v4}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v3, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    invoke-virtual {p0, v2}, Lorg/json/JSONArray;->put(Ljava/lang/Object;)Lorg/json/JSONArray;

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_0
    return-void
.end method

.method private b(Landroid/os/Bundle;)Lorg/json/JSONObject;
    .locals 6

    new-instance v1, Lorg/json/JSONObject;

    invoke-direct {v1}, Lorg/json/JSONObject;-><init>()V

    :try_start_0
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    const-string v2, "key"

    const-string v3, "DEVELOPER_KEY"

    invoke-virtual {v0, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v2, "value"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDeveloperKey()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v2, "app_id"

    invoke-virtual {v1, v2, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    new-instance v2, Lorg/json/JSONArray;

    invoke-direct {v2}, Lorg/json/JSONArray;-><init>()V

    sget-object v0, Lcom/kamcord/android/KC_m;->a:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v3

    :cond_0
    :goto_0
    invoke-interface {v3}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v3}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_m$KC_a;

    iget-object v4, v0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    invoke-virtual {p1, v4}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_0

    new-instance v4, Lorg/json/JSONObject;

    iget-object v0, v0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-direct {v4, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    invoke-static {v2, v4}, Lcom/kamcord/android/KC_c;->a(Lorg/json/JSONArray;Lorg/json/JSONObject;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    :goto_1
    return-object v1

    :cond_1
    :try_start_1
    const-string v0, "event"

    invoke-virtual {v1, v0, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "device_time"

    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v2

    const-wide/16 v4, 0x3e8

    div-long/2addr v2, v4

    invoke-virtual {v1, v0, v2, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;J)Lorg/json/JSONObject;
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_0

    goto :goto_1
.end method


# virtual methods
.method protected final a()Lcom/kamcord/android/KC_ac;
    .locals 2

    new-instance v0, Lcom/kamcord/android/KC_b;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_c;->getLooper()Landroid/os/Looper;

    move-result-object v1

    invoke-direct {v0, v1, p0}, Lcom/kamcord/android/KC_b;-><init>(Landroid/os/Looper;Lcom/kamcord/android/KC_c;)V

    return-object v0
.end method

.method public final a(Landroid/os/Bundle;)V
    .locals 9

    const/16 v8, 0xc8

    const/4 v1, 0x0

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_c;->b(Landroid/os/Bundle;)Lorg/json/JSONObject;

    move-result-object v0

    new-instance v2, Lorg/apache/http/params/BasicHttpParams;

    invoke-direct {v2}, Lorg/apache/http/params/BasicHttpParams;-><init>()V

    sget v3, Lcom/kamcord/android/KC_c;->c:I

    invoke-static {v2, v3}, Lorg/apache/http/params/HttpConnectionParams;->setConnectionTimeout(Lorg/apache/http/params/HttpParams;I)V

    sget v3, Lcom/kamcord/android/KC_c;->c:I

    invoke-static {v2, v3}, Lorg/apache/http/params/HttpConnectionParams;->setSoTimeout(Lorg/apache/http/params/HttpParams;I)V

    new-instance v5, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v5, v2}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>(Lorg/apache/http/params/HttpParams;)V

    const-string v2, "trackEvent"

    invoke-static {v2, v0}, Lcom/kamcord/android/KC_d;->a(Ljava/lang/String;Lorg/json/JSONObject;)Lorg/apache/http/client/methods/HttpPost;

    move-result-object v6

    const/4 v2, 0x0

    move v0, v1

    move v3, v1

    :goto_0
    sget v4, Lcom/kamcord/android/KC_c;->b:I

    if-ge v3, v4, :cond_7

    :try_start_0
    invoke-interface {v5, v6}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    move-result-object v0

    :try_start_1
    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v2

    invoke-interface {v2}, Lorg/apache/http/StatusLine;->getStatusCode()I
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0

    move-result v2

    if-ne v2, v8, :cond_2

    :goto_1
    sget v2, Lcom/kamcord/android/KC_c;->b:I

    if-gt v3, v2, :cond_0

    if-eqz v1, :cond_6

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "After trying "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget v1, Lcom/kamcord/android/KC_c;->b:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " times, unable to connect to analytics. Giving up."

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/KC_m;->c()V

    :cond_1
    :goto_2
    return-void

    :cond_2
    :try_start_2
    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v2

    invoke-interface {v2}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v2

    const/16 v4, 0x1f4

    if-ne v2, v4, :cond_5

    new-instance v2, Lorg/json/JSONObject;

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v4

    invoke-static {v4}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v2, v4}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    const-string v4, "status_reason"

    invoke-virtual {v2, v4}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_3

    new-instance v4, Ljava/lang/StringBuilder;

    const-string v7, "Response from analytics not OK, status_reason: "

    invoke-direct {v4, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    const-string v7, "status_reason"

    invoke-virtual {v2, v7}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v7

    invoke-virtual {v4, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_3
    const-string v4, "retriable"

    invoke-virtual {v2, v4}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_4

    const-string v4, "retriable"

    invoke-virtual {v2, v4}, Lorg/json/JSONObject;->getBoolean(Ljava/lang/String;)Z

    move-result v2

    if-nez v2, :cond_4

    sget v3, Lcom/kamcord/android/KC_c;->b:I

    goto :goto_1

    :cond_4
    move-object v2, v0

    move v0, v1

    :goto_3
    add-int/lit8 v3, v3, 0x1

    goto/16 :goto_0

    :cond_5
    const-string v2, "Response from analytics not OK, retrying..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_0

    move-object v2, v0

    move v0, v1

    goto :goto_3

    :catch_0
    move-exception v2

    move-object v4, v2

    move-object v2, v0

    :goto_4
    const/4 v0, 0x1

    const-string v7, "Something went wrong connecting to analytics, retrying..."

    invoke-static {v7}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v4}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_3

    :cond_6
    if-eqz v0, :cond_1

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v0

    if-ne v0, v8, :cond_1

    invoke-static {}, Lcom/kamcord/android/KC_m;->b()V

    goto :goto_2

    :catch_1
    move-exception v0

    move-object v4, v0

    goto :goto_4

    :cond_7
    move v1, v0

    move-object v0, v2

    goto/16 :goto_1
.end method
