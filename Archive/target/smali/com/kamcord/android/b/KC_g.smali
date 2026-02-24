.class public final Lcom/kamcord/android/b/KC_g;
.super Lcom/kamcord/android/b/KC_e;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/b/KC_e;-><init>()V

    return-void
.end method

.method public static a(Z)Lcom/kamcord/a/a/d/KC_j;
    .locals 6

    const/4 v0, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v2

    const-string v1, "youtube_refresh_token"

    invoke-interface {v2, v1, v0}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    if-eqz v1, :cond_2

    invoke-static {}, Lcom/kamcord/android/b/KC_g;->h()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    new-instance v3, Lcom/kamcord/a/a/d/KC_l;

    invoke-direct {v3, v1}, Lcom/kamcord/a/a/d/KC_l;-><init>(Ljava/lang/String;)V

    invoke-interface {v0, v3}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v1

    invoke-interface {v2}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v3

    invoke-interface {v0, v1}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_j;)Ljava/util/HashMap;

    move-result-object v0

    if-eqz v0, :cond_1

    invoke-virtual {v0}, Ljava/util/HashMap;->entrySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v4

    :cond_0
    :goto_0
    invoke-interface {v4}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v4}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    const-string v5, "youtube_refresh_token"

    invoke-virtual {v1, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-nez v1, :cond_0

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    invoke-interface {v3, v1, v0}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    goto :goto_0

    :cond_1
    invoke-interface {v3}, Landroid/content/SharedPreferences$Editor;->apply()V

    new-instance v0, Lcom/kamcord/a/a/d/KC_j;

    const-string v1, "youtube_token"

    const-string v3, ""

    invoke-interface {v2, v1, v3}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v2, ""

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_j;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    :cond_2
    return-object v0
.end method

.method private static h()Lcom/kamcord/a/a/e/KC_c;
    .locals 2

    new-instance v0, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v0}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-class v1, Lcom/kamcord/a/a/a/a/KC_e;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "923796282600.apps.googleusercontent.com"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "G1PEm726K2cvrIf9NmTHHTAe"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "https://gdata.youtube.com https://www.googleapis.com/auth/userinfo.profile"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->d(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "http://localhost"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    return-object v0
.end method


# virtual methods
.method public final a()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "youtube_token"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "youtube_refresh_token"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "youtube_expiration"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "YouTubeUsername"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    const-string v1, "YouTube"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Z)V

    return-void
.end method

.method public final a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final a(Landroid/content/Context;)V
    .locals 12

    const/4 v1, 0x0

    const/4 v5, 0x0

    invoke-static {}, Lcom/kamcord/android/b/KC_g;->h()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v3, "youtube_refresh_token"

    invoke-interface {v0, v3, v5}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    if-eqz v3, :cond_2

    const-string v4, "youtube_expiration"

    invoke-interface {v0, v4, v5}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    const/4 v0, 0x1

    if-eqz v4, :cond_0

    :try_start_0
    invoke-static {v4}, Ljava/lang/Long;->parseLong(Ljava/lang/String;)J

    move-result-wide v6

    const-string v5, "UTC"

    invoke-static {v5}, Ljava/util/TimeZone;->getTimeZone(Ljava/lang/String;)Ljava/util/TimeZone;

    move-result-object v5

    invoke-static {v5}, Ljava/util/Calendar;->getInstance(Ljava/util/TimeZone;)Ljava/util/Calendar;

    move-result-object v5

    invoke-virtual {v5}, Ljava/util/Calendar;->getTimeInMillis()J

    move-result-wide v8

    const-wide/16 v10, 0x3e8

    div-long/2addr v8, v10

    const-wide/16 v10, 0x4b0

    add-long/2addr v8, v10

    invoke-static {v8, v9}, Ljava/lang/Long;->valueOf(J)Ljava/lang/Long;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/Long;->longValue()J
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-wide v4

    cmp-long v4, v6, v4

    if-lez v4, :cond_0

    move v0, v1

    :cond_0
    :goto_0
    if-eqz v0, :cond_1

    new-instance v0, Lcom/kamcord/a/a/d/KC_l;

    invoke-direct {v0, v3}, Lcom/kamcord/a/a/d/KC_l;-><init>(Ljava/lang/String;)V

    new-instance v3, Lcom/kamcord/android/KC_B;

    invoke-direct {v3, v2, v0}, Lcom/kamcord/android/KC_B;-><init>(Lcom/kamcord/a/a/e/KC_c;Lcom/kamcord/a/a/d/KC_l;)V

    new-array v0, v1, [Ljava/lang/Void;

    invoke-virtual {v3, v0}, Lcom/kamcord/android/KC_B;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    :cond_1
    :goto_1
    return-void

    :catch_0
    move-exception v5

    sget-object v5, Ljava/lang/System;->out:Ljava/io/PrintStream;

    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "Could not parse expiration: "

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v6, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v5, v4}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    goto :goto_0

    :cond_2
    new-instance v0, Landroid/content/Intent;

    const-class v1, Lcom/kamcord/android/WebActivity;

    invoke-direct {v0, p1, v1}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v1, "loginUrl"

    invoke-interface {v2, v5}, Lcom/kamcord/a/a/e/KC_c;->b(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "class"

    const-class v3, Lcom/kamcord/a/a/a/a/KC_e;

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/io/Serializable;)Landroid/content/Intent;

    const-string v1, "apiKey"

    const-string v3, "923796282600.apps.googleusercontent.com"

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "apiSecret"

    const-string v3, "G1PEm726K2cvrIf9NmTHHTAe"

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "scope"

    const-string v3, "https://gdata.youtube.com https://www.googleapis.com/auth/userinfo.profile"

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "callback"

    const-string v3, "http://localhost"

    invoke-virtual {v0, v1, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v1, "oauth"

    invoke-interface {v2}, Lcom/kamcord/a/a/e/KC_c;->e()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    invoke-virtual {p1, v0}, Landroid/content/Context;->startActivity(Landroid/content/Intent;)V

    goto :goto_1
.end method

.method public final b()Z
    .locals 4

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v1

    const-string v0, "youtube_token"

    invoke-interface {v1, v0}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    new-instance v0, Lcom/kamcord/a/a/d/KC_j;

    const-string v2, "youtube_token"

    const-string v3, ""

    invoke-interface {v1, v2, v3}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v2, ""

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_j;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    :goto_0
    if-eqz v0, :cond_1

    const/4 v0, 0x1

    :goto_1
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0

    :cond_1
    const/4 v0, 0x0

    goto :goto_1
.end method

.method public final d()Ljava/lang/String;
    .locals 1

    const-string v0, "YouTube"

    return-object v0
.end method
