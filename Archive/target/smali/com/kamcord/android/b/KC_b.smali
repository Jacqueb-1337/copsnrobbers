.class public final Lcom/kamcord/android/b/KC_b;
.super Lcom/kamcord/android/b/KC_e;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/b/KC_e;-><init>()V

    return-void
.end method

.method private static h()Lcom/kamcord/a/a/d/KC_j;
    .locals 10

    const/4 v1, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v2

    const-string v0, "facebook_expiration"

    invoke-interface {v2, v0, v1}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const/4 v0, 0x1

    if-eqz v3, :cond_0

    :try_start_0
    invoke-static {v3}, Ljava/lang/Long;->parseLong(Ljava/lang/String;)J

    move-result-wide v4

    const-string v6, "UTC"

    invoke-static {v6}, Ljava/util/TimeZone;->getTimeZone(Ljava/lang/String;)Ljava/util/TimeZone;

    move-result-object v6

    invoke-static {v6}, Ljava/util/Calendar;->getInstance(Ljava/util/TimeZone;)Ljava/util/Calendar;

    move-result-object v6

    invoke-virtual {v6}, Ljava/util/Calendar;->getTimeInMillis()J

    move-result-wide v6

    const-wide/16 v8, 0x3e8

    div-long/2addr v6, v8

    const-wide/16 v8, 0x4b0

    add-long/2addr v6, v8

    invoke-static {v6, v7}, Ljava/lang/Long;->valueOf(J)Ljava/lang/Long;

    move-result-object v6

    invoke-virtual {v6}, Ljava/lang/Long;->longValue()J
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-wide v6

    cmp-long v3, v4, v6

    if-lez v3, :cond_0

    const/4 v0, 0x0

    :cond_0
    :goto_0
    if-eqz v0, :cond_1

    move-object v0, v1

    :goto_1
    return-object v0

    :catch_0
    move-exception v4

    sget-object v4, Ljava/lang/System;->out:Ljava/io/PrintStream;

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "Could not parse expiration: "

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v4, v3}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    goto :goto_0

    :cond_1
    new-instance v0, Lcom/kamcord/a/a/d/KC_j;

    const-string v1, "facebook_token"

    const-string v3, ""

    invoke-interface {v2, v1, v3}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v2, ""

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_j;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_1
.end method


# virtual methods
.method public final a()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "facebook_token"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "facebook_expiration"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "FacebookUsername"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    const-string v1, "Facebook"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Z)V

    return-void
.end method

.method public final a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 5

    if-nez p3, :cond_0

    const-string p3, ""

    :cond_0
    new-instance v0, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v0}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-class v1, Lcom/kamcord/a/a/a/a/KC_d;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "386441044729315"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "59786de74164a66011887ee1f7d74103"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "publish_actions"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->d(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "http://kamcord.com/"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v1

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "https://www.kamcord.com/v/"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    new-instance v2, Lcom/kamcord/a/a/d/KC_c;

    sget-object v3, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    const-string v4, "https://graph.facebook.com/v2.0/me/feed"

    invoke-direct {v2, v3, v4}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    const-string v3, "link"

    invoke-virtual {v2, v3, v0}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "message"

    invoke-virtual {v2, v0, p3}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/b/KC_b;->h()Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    if-nez v0, :cond_1

    sget-object v0, Lcom/kamcord/a/a/d/KC_b;->a:Lcom/kamcord/a/a/d/KC_j;

    :cond_1
    const-string v3, "access_token"

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v3, v0}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    new-instance v0, Lcom/kamcord/android/KC_C;

    invoke-direct {v0, v2, v1, p2}, Lcom/kamcord/android/KC_C;-><init>(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/e/KC_c;Ljava/lang/String;)V

    const/4 v1, 0x0

    new-array v1, v1, [Ljava/lang/Void;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_C;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    return-void
.end method

.method public final a(Landroid/content/Context;)V
    .locals 9

    const/4 v8, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "facebook_token"

    invoke-interface {v0, v1, v8}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    if-eqz v1, :cond_0

    const-string v1, "facebook_expiration"

    invoke-interface {v0, v1, v8}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    if-eqz v0, :cond_0

    :try_start_0
    invoke-static {v0}, Ljava/lang/Long;->parseLong(Ljava/lang/String;)J

    move-result-wide v2

    const-string v1, "UTC"

    invoke-static {v1}, Ljava/util/TimeZone;->getTimeZone(Ljava/lang/String;)Ljava/util/TimeZone;

    move-result-object v1

    invoke-static {v1}, Ljava/util/Calendar;->getInstance(Ljava/util/TimeZone;)Ljava/util/Calendar;

    move-result-object v1

    invoke-virtual {v1}, Ljava/util/Calendar;->getTimeInMillis()J

    move-result-wide v4

    const-wide/16 v6, 0x3e8

    div-long/2addr v4, v6

    const-wide/16 v6, 0x4b0

    add-long/2addr v4, v6

    invoke-static {v4, v5}, Ljava/lang/Long;->valueOf(J)Ljava/lang/Long;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/Long;->longValue()J
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-wide v0

    cmp-long v0, v2, v0

    if-lez v0, :cond_0

    :goto_0
    return-void

    :catch_0
    move-exception v1

    sget-object v1, Ljava/lang/System;->out:Ljava/io/PrintStream;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Could not parse expiration: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v0}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    :cond_0
    new-instance v0, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v0}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-class v1, Lcom/kamcord/a/a/a/a/KC_d;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "386441044729315"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "59786de74164a66011887ee1f7d74103"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "publish_actions"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->d(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "http://kamcord.com/"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    new-instance v1, Landroid/content/Intent;

    const-class v2, Lcom/kamcord/android/WebActivity;

    invoke-direct {v1, p1, v2}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    const-string v2, "loginUrl"

    invoke-interface {v0, v8}, Lcom/kamcord/a/a/e/KC_c;->b(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v2, "class"

    const-class v3, Lcom/kamcord/a/a/a/a/KC_d;

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/io/Serializable;)Landroid/content/Intent;

    const-string v2, "apiKey"

    const-string v3, "386441044729315"

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v2, "apiSecret"

    const-string v3, "59786de74164a66011887ee1f7d74103"

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v2, "scope"

    const-string v3, "publish_actions"

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v2, "callback"

    const-string v3, "http://kamcord.com/"

    invoke-virtual {v1, v2, v3}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    const-string v2, "oauth"

    invoke-interface {v0}, Lcom/kamcord/a/a/e/KC_c;->e()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v2, v0}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Ljava/lang/String;)Landroid/content/Intent;

    invoke-virtual {p1, v1}, Landroid/content/Context;->startActivity(Landroid/content/Intent;)V

    goto :goto_0
.end method

.method public final b()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/b/KC_b;->h()Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final d()Ljava/lang/String;
    .locals 1

    const-string v0, "Facebook"

    return-object v0
.end method
