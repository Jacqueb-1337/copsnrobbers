.class final Lcom/kamcord/a/a/a/a/KC_e$KC_a;
.super Lcom/kamcord/a/a/e/KC_b;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/a/a/a/a/KC_e;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private a:Lcom/kamcord/a/a/a/a/KC_c;

.field private b:Lcom/kamcord/a/a/d/KC_a;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/a/a/KC_e;Lcom/kamcord/a/a/a/a/KC_c;Lcom/kamcord/a/a/d/KC_a;)V
    .locals 0

    invoke-direct {p0, p2, p3}, Lcom/kamcord/a/a/e/KC_b;-><init>(Lcom/kamcord/a/a/a/a/KC_c;Lcom/kamcord/a/a/d/KC_a;)V

    iput-object p2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    iput-object p3, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
    .locals 3

    new-instance v0, Lcom/kamcord/a/a/d/KC_c;

    iget-object v1, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->b()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v2}, Lcom/kamcord/a/a/a/a/KC_c;->c()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    sget-object v1, Lcom/kamcord/a/a/a/a/KC_e$2;->a:[I

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v2}, Lcom/kamcord/a/a/a/a/KC_c;->b()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_k;->ordinal()I

    move-result v2

    aget v1, v1, v2

    packed-switch v1, :pswitch_data_0

    const-string v1, "client_id"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "client_secret"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->b()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "code"

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_l;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "redirect_uri"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_a;->f()Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "scope"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->e()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    :cond_0
    :goto_0
    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_c;->b()Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->a()Lcom/kamcord/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v0

    invoke-interface {v1, v0}, Lcom/kamcord/a/a/c/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0

    :pswitch_0
    const-string v1, "client_id"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "client_secret"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->b()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "code"

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_l;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "redirect_uri"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "grant_type"

    const-string v2, "authorization_code"

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
    .end packed-switch
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
    .locals 3

    new-instance v0, Lcom/kamcord/a/a/d/KC_c;

    iget-object v1, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->b()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v2}, Lcom/kamcord/a/a/a/a/KC_c;->c()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    sget-object v1, Lcom/kamcord/a/a/a/a/KC_e$2;->a:[I

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v2}, Lcom/kamcord/a/a/a/a/KC_c;->b()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_k;->ordinal()I

    move-result v2

    aget v1, v1, v2

    packed-switch v1, :pswitch_data_0

    :goto_0
    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_c;->b()Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->a()Z

    move-result v1

    if-nez v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v1

    invoke-virtual {v1, p0}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/a/a/e/KC_c;)V

    :cond_0
    iget-object v1, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->a()Lcom/kamcord/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v0

    invoke-interface {v1, v0}, Lcom/kamcord/a/a/c/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0

    :pswitch_0
    const-string v1, "client_id"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "client_secret"

    iget-object v2, p0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->b()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "refresh_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_l;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "grant_type"

    const-string v2, "refresh_token"

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
    .end packed-switch
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_h;)Ljava/lang/String;
    .locals 3

    const/4 v0, 0x0

    :try_start_0
    new-instance v1, Lorg/json/JSONObject;

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    const-string v2, "name"

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->has(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_0

    const-string v2, "name"

    invoke-virtual {v1, v2}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    :cond_0
    :goto_0
    return-object v0

    :catch_0
    move-exception v1

    invoke-virtual {v1}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_j;)Ljava/util/HashMap;
    .locals 6
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/a/a/d/KC_j;",
            ")",
            "Ljava/util/HashMap",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation

    new-instance v1, Ljava/util/HashMap;

    invoke-direct {v1}, Ljava/util/HashMap;-><init>()V

    const-string v0, "youtube_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v0, v2}, Ljava/util/HashMap;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    const-string v0, "youtube_refresh_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->d()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v0, v2}, Ljava/util/HashMap;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    const/4 v0, 0x0

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->e()Ljava/lang/String;

    move-result-object v2

    if-eqz v2, :cond_0

    :try_start_0
    const-string v2, "UTC"

    invoke-static {v2}, Ljava/util/TimeZone;->getTimeZone(Ljava/lang/String;)Ljava/util/TimeZone;

    move-result-object v2

    invoke-static {v2}, Ljava/util/Calendar;->getInstance(Ljava/util/TimeZone;)Ljava/util/Calendar;

    move-result-object v2

    invoke-virtual {v2}, Ljava/util/Calendar;->getTimeInMillis()J

    move-result-wide v2

    const-wide/16 v4, 0x3e8

    div-long/2addr v2, v4

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->e()Ljava/lang/String;

    move-result-object v4

    invoke-static {v4}, Ljava/lang/Long;->parseLong(Ljava/lang/String;)J

    move-result-wide v4

    add-long/2addr v2, v4

    invoke-static {v2, v3}, Ljava/lang/Long;->valueOf(J)Ljava/lang/Long;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/Long;->toString()Ljava/lang/String;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    :cond_0
    :goto_0
    const-string v2, "youtube_expiration"

    invoke-virtual {v1, v2, v0}, Ljava/util/HashMap;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    return-object v1

    :catch_0
    move-exception v2

    sget-object v2, Ljava/lang/System;->out:Ljava/io/PrintStream;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "Invalid expiration: "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->e()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final a()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    const-string v1, "YouTube"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Z)V

    return-void
.end method

.method public final b()Ljava/lang/String;
    .locals 1

    const-string v0, "YouTube"

    return-object v0
.end method

.method public final c()Ljava/lang/String;
    .locals 1

    const-string v0, "https://www.googleapis.com/oauth2/v3/userinfo"

    return-object v0
.end method
