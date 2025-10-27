.class public Lcom/kamcord/a/a/d/KC_f;
.super Ljava/lang/Object;


# static fields
.field private static a:Lcom/kamcord/a/a/d/KC_g;


# instance fields
.field private b:Ljava/lang/String;

.field private c:Lcom/kamcord/a/a/d/KC_k;

.field private d:Lcom/kamcord/a/a/d/KC_e;

.field private e:Lcom/kamcord/a/a/d/KC_e;

.field private f:Ljava/util/Map;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Map",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation
.end field

.field private g:Ljava/lang/String;

.field private h:Ljava/net/HttpURLConnection;

.field private i:[B

.field private j:Z

.field private k:Z

.field private l:Ljava/lang/Long;

.field private m:Ljava/lang/Long;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/d/KC_f$1;

    invoke-direct {v0}, Lcom/kamcord/a/a/d/KC_f$1;-><init>()V

    sput-object v0, Lcom/kamcord/a/a/d/KC_f;->a:Lcom/kamcord/a/a/d/KC_g;

    return-void
.end method

.method public constructor <init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->g:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->i:[B

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/a/a/d/KC_f;->j:Z

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/a/a/d/KC_f;->k:Z

    iput-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->l:Ljava/lang/Long;

    iput-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->m:Ljava/lang/Long;

    iput-object p1, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    iput-object p2, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    new-instance v0, Lcom/kamcord/a/a/d/KC_e;

    invoke-direct {v0}, Lcom/kamcord/a/a/d/KC_e;-><init>()V

    iput-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->d:Lcom/kamcord/a/a/d/KC_e;

    new-instance v0, Lcom/kamcord/a/a/d/KC_e;

    invoke-direct {v0}, Lcom/kamcord/a/a/d/KC_e;-><init>()V

    iput-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->e:Lcom/kamcord/a/a/d/KC_e;

    new-instance v0, Ljava/util/HashMap;

    invoke-direct {v0}, Ljava/util/HashMap;-><init>()V

    iput-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->f:Ljava/util/Map;

    return-void
.end method

.method private a(Ljava/net/HttpURLConnection;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->f:Ljava/util/Map;

    invoke-interface {v0}, Ljava/util/Map;->keySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_0
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->f:Ljava/util/Map;

    invoke-interface {v1, v0}, Ljava/util/Map;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Ljava/net/HttpURLConnection;->setRequestProperty(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method private a()[B
    .locals 4

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->e:Lcom/kamcord/a/a/d/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_e;->a()Ljava/lang/String;

    move-result-object v0

    :try_start_0
    invoke-static {}, Ljava/nio/charset/Charset;->defaultCharset()Ljava/nio/charset/Charset;

    move-result-object v1

    invoke-virtual {v1}, Ljava/nio/charset/Charset;->name()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/String;->getBytes(Ljava/lang/String;)[B
    :try_end_0
    .catch Ljava/io/UnsupportedEncodingException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    return-object v0

    :catch_0
    move-exception v0

    new-instance v1, Lcom/kamcord/a/a/b/KC_b;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unsupported Charset: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Ljava/nio/charset/Charset;->defaultCharset()Ljava/nio/charset/Charset;

    move-result-object v3

    invoke-virtual {v3}, Ljava/nio/charset/Charset;->name()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/b/KC_b;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1
.end method


# virtual methods
.method public final a(Lcom/kamcord/a/a/d/KC_g;)Lcom/kamcord/a/a/d/KC_h;
    .locals 4

    :try_start_0
    invoke-virtual {p0}, Lcom/kamcord/a/a/d/KC_f;->c()Ljava/lang/String;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    if-nez v1, :cond_0

    const-string v1, "http.keepAlive"

    const-string v2, "false"

    invoke-static {v1, v2}, Ljava/lang/System;->setProperty(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    new-instance v1, Ljava/net/URL;

    invoke-direct {v1, v0}, Ljava/net/URL;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1}, Ljava/net/URL;->openConnection()Ljava/net/URLConnection;

    move-result-object v0

    check-cast v0, Ljava/net/HttpURLConnection;

    iput-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    iget-boolean v1, p0, Lcom/kamcord/a/a/d/KC_f;->k:Z

    invoke-virtual {v0, v1}, Ljava/net/HttpURLConnection;->setInstanceFollowRedirects(Z)V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_k;->name()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/net/HttpURLConnection;->setRequestMethod(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->m:Ljava/lang/Long;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->m:Ljava/lang/Long;

    invoke-virtual {v1}, Ljava/lang/Long;->intValue()I

    move-result v1

    invoke-virtual {v0, v1}, Ljava/net/HttpURLConnection;->setReadTimeout(I)V

    :cond_1
    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    invoke-direct {p0, v0}, Lcom/kamcord/a/a/d/KC_f;->a(Ljava/net/HttpURLConnection;)V

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    sget-object v1, Lcom/kamcord/a/a/d/KC_k;->c:Lcom/kamcord/a/a/d/KC_k;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_k;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    sget-object v1, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_k;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_4

    :cond_2
    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    invoke-direct {p0}, Lcom/kamcord/a/a/d/KC_f;->a()[B

    move-result-object v1

    const-string v2, "Content-Length"

    array-length v3, v1

    invoke-static {v3}, Ljava/lang/String;->valueOf(I)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v2, v3}, Ljava/net/HttpURLConnection;->setRequestProperty(Ljava/lang/String;Ljava/lang/String;)V

    const-string v2, "Content-Type"

    invoke-virtual {v0, v2}, Ljava/net/HttpURLConnection;->getRequestProperty(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    if-nez v2, :cond_3

    const-string v2, "Content-Type"

    const-string v3, "application/x-www-form-urlencoded"

    invoke-virtual {v0, v2, v3}, Ljava/net/HttpURLConnection;->setRequestProperty(Ljava/lang/String;Ljava/lang/String;)V

    :cond_3
    const/4 v2, 0x1

    invoke-virtual {v0, v2}, Ljava/net/HttpURLConnection;->setDoOutput(Z)V

    invoke-virtual {v0}, Ljava/net/HttpURLConnection;->getOutputStream()Ljava/io/OutputStream;

    move-result-object v0

    invoke-virtual {v0, v1}, Ljava/io/OutputStream;->write([B)V

    :cond_4
    invoke-virtual {p1, p0}, Lcom/kamcord/a/a/d/KC_g;->a(Lcom/kamcord/a/a/d/KC_f;)V

    new-instance v0, Lcom/kamcord/a/a/d/KC_h;

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->h:Ljava/net/HttpURLConnection;

    invoke-direct {v0, v1}, Lcom/kamcord/a/a/d/KC_h;-><init>(Ljava/net/HttpURLConnection;)V
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    return-object v0

    :catch_0
    move-exception v0

    new-instance v1, Lcom/kamcord/a/a/b/KC_a;

    invoke-direct {v1, v0}, Lcom/kamcord/a/a/b/KC_a;-><init>(Ljava/lang/Exception;)V

    throw v1
.end method

.method public final a(ILjava/util/concurrent/TimeUnit;)V
    .locals 2

    int-to-long v0, p1

    invoke-virtual {p2, v0, v1}, Ljava/util/concurrent/TimeUnit;->toMillis(J)J

    move-result-wide v0

    invoke-static {v0, v1}, Ljava/lang/Long;->valueOf(J)Ljava/lang/Long;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->m:Ljava/lang/Long;

    return-void
.end method

.method public final b()Lcom/kamcord/a/a/d/KC_h;
    .locals 1

    sget-object v0, Lcom/kamcord/a/a/d/KC_f;->a:Lcom/kamcord/a/a/d/KC_g;

    invoke-virtual {p0, v0}, Lcom/kamcord/a/a/d/KC_f;->a(Lcom/kamcord/a/a/d/KC_g;)Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    return-object v0
.end method

.method public final b(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->f:Ljava/util/Map;

    invoke-interface {v0, p1, p2}, Ljava/util/Map;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    return-void
.end method

.method public final c()Ljava/lang/String;
    .locals 2

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->d:Lcom/kamcord/a/a/d/KC_e;

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_e;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public final c(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->e:Lcom/kamcord/a/a/d/KC_e;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/a/a/d/KC_e;->a(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method public final d()Lcom/kamcord/a/a/d/KC_e;
    .locals 3

    :try_start_0
    new-instance v0, Lcom/kamcord/a/a/d/KC_e;

    invoke-direct {v0}, Lcom/kamcord/a/a/d/KC_e;-><init>()V

    new-instance v1, Ljava/net/URL;

    iget-object v2, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    invoke-direct {v1, v2}, Ljava/net/URL;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1}, Ljava/net/URL;->getQuery()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_e;->b(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_f;->d:Lcom/kamcord/a/a/d/KC_e;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_e;->a(Lcom/kamcord/a/a/d/KC_e;)V
    :try_end_0
    .catch Ljava/net/MalformedURLException; {:try_start_0 .. :try_end_0} :catch_0

    return-object v0

    :catch_0
    move-exception v0

    new-instance v1, Lcom/kamcord/a/a/b/KC_b;

    const-string v2, "Malformed URL"

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/b/KC_b;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1
.end method

.method public final d(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->d:Lcom/kamcord/a/a/d/KC_e;

    invoke-virtual {v0, p1, p2}, Lcom/kamcord/a/a/d/KC_e;->a(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method public final e()Lcom/kamcord/a/a/d/KC_e;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->e:Lcom/kamcord/a/a/d/KC_e;

    return-object v0
.end method

.method public final f()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    return-object v0
.end method

.method public final g()Ljava/lang/String;
    .locals 3

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    const-string v1, "\\?.*"

    const-string v2, ""

    invoke-virtual {v0, v1, v2}, Ljava/lang/String;->replaceAll(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    const-string v1, "\\:\\d{4}"

    const-string v2, ""

    invoke-virtual {v0, v1, v2}, Ljava/lang/String;->replace(Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public final h()Lcom/kamcord/a/a/d/KC_k;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    return-object v0
.end method

.method public toString()Ljava/lang/String;
    .locals 4

    const-string v0, "@Request(%s %s)"

    const/4 v1, 0x2

    new-array v1, v1, [Ljava/lang/Object;

    const/4 v2, 0x0

    iget-object v3, p0, Lcom/kamcord/a/a/d/KC_f;->c:Lcom/kamcord/a/a/d/KC_k;

    aput-object v3, v1, v2

    const/4 v2, 0x1

    iget-object v3, p0, Lcom/kamcord/a/a/d/KC_f;->b:Ljava/lang/String;

    aput-object v3, v1, v2

    invoke-static {v0, v1}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
