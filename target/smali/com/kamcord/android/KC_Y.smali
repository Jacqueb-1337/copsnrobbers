.class public final Lcom/kamcord/android/KC_Y;
.super Ljava/lang/Thread;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_Y$1;,
        Lcom/kamcord/android/KC_Y$KC_b;,
        Lcom/kamcord/android/KC_Y$KC_a;,
        Lcom/kamcord/android/KC_Y$KC_c;,
        Lcom/kamcord/android/KC_Y$KC_d;
    }
.end annotation


# static fields
.field public static a:Ljava/lang/String;

.field private static e:Ljava/util/Set;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Set",
            "<",
            "Lcom/kamcord/android/KC_X;",
            ">;"
        }
    .end annotation
.end field

.field private static j:Lorg/xmlpull/v1/XmlPullParserFactory;


# instance fields
.field private A:J

.field private b:Lcom/kamcord/android/KC_Y$KC_d;

.field private c:Ljava/lang/ref/WeakReference;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/lang/ref/WeakReference",
            "<",
            "Lcom/kamcord/android/UploadService;",
            ">;"
        }
    .end annotation
.end field

.field private d:Landroid/content/Intent;

.field private f:I

.field private g:J

.field private h:Ljava/io/FileInputStream;

.field private i:J

.field private k:Ljava/lang/String;

.field private l:Ljava/lang/String;

.field private m:Ljava/lang/String;

.field private n:Ljava/lang/String;

.field private o:Ljava/lang/String;

.field private p:Ljava/lang/String;

.field private q:Ljava/lang/String;

.field private r:Ljava/lang/String;

.field private s:Ljava/lang/String;

.field private t:Ljava/lang/String;

.field private u:J

.field private v:I

.field private w:[Ljava/lang/String;

.field private x:Ljava/lang/String;

.field private y:Z

.field private z:J


# direct methods
.method static constructor <clinit>()V
    .locals 2

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sput-object v0, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    const/4 v0, 0x0

    sput-object v0, Lcom/kamcord/android/KC_Y;->a:Ljava/lang/String;

    :try_start_0
    invoke-static {}, Lorg/xmlpull/v1/XmlPullParserFactory;->newInstance()Lorg/xmlpull/v1/XmlPullParserFactory;

    move-result-object v0

    sput-object v0, Lcom/kamcord/android/KC_Y;->j:Lorg/xmlpull/v1/XmlPullParserFactory;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lorg/xmlpull/v1/XmlPullParserFactory;->setNamespaceAware(Z)V
    :try_end_0
    .catch Lorg/xmlpull/v1/XmlPullParserException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Unexpected exception during XML parser initialization..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/xmlpull/v1/XmlPullParserException;->printStackTrace()V

    goto :goto_0
.end method

.method public constructor <init>(Lcom/kamcord/android/UploadService;Landroid/content/Intent;)V
    .locals 6

    const-wide/16 v4, 0x0

    const/4 v2, 0x0

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Thread;-><init>()V

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->a:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    iput v2, p0, Lcom/kamcord/android/KC_Y;->f:I

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->g:J

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->i:J

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->q:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->r:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->s:Ljava/lang/String;

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->t:Ljava/lang/String;

    iput v2, p0, Lcom/kamcord/android/KC_Y;->v:I

    iput-object v1, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    iput-boolean v2, p0, Lcom/kamcord/android/KC_Y;->y:Z

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->z:J

    const-wide/32 v0, 0x3b9aca00

    iput-wide v0, p0, Lcom/kamcord/android/KC_Y;->A:J

    new-instance v0, Ljava/lang/ref/WeakReference;

    invoke-direct {v0, p1}, Ljava/lang/ref/WeakReference;-><init>(Ljava/lang/Object;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->c:Ljava/lang/ref/WeakReference;

    iput-object p2, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "local_video_id"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "is_reshare"

    invoke-virtual {v0, v1, v2}, Landroid/content/Intent;->getBooleanExtra(Ljava/lang/String;Z)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_Y;->y:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "server_video_id"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "server_video_id"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    iget-boolean v0, p0, Lcom/kamcord/android/KC_Y;->y:Z

    if-eqz v0, :cond_2

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->i:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "shared_on"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "shared_on"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getSerializableExtra(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Ljava/util/HashSet;

    const-string v1, ""

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v3, "message"

    invoke-virtual {v2, v3}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v2, "message"

    invoke-virtual {v1, v2}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    :cond_0
    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-static {v0, v2, v3, v1}, Lcom/kamcord/android/KC_Y;->a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    :cond_1
    :goto_0
    return-void

    :cond_2
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->c:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    goto :goto_0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_Y;J)J
    .locals 3

    iget-wide v0, p0, Lcom/kamcord/android/KC_Y;->g:J

    add-long/2addr v0, p1

    iput-wide v0, p0, Lcom/kamcord/android/KC_Y;->g:J

    return-wide v0
.end method

.method private a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;
    .locals 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "\n\n"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "\n"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "\nx-amz-security-token:"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->m:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "\n/"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "/"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    if-eqz p6, :cond_0

    const-string v1, ""

    invoke-virtual {p6, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-nez v1, :cond_0

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "?"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    :cond_0
    new-instance v1, Ljavax/crypto/spec/SecretKeySpec;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->k:Ljava/lang/String;

    invoke-virtual {v2}, Ljava/lang/String;->getBytes()[B

    move-result-object v2

    const-string v3, "HmacSHA1"

    invoke-direct {v1, v2, v3}, Ljavax/crypto/spec/SecretKeySpec;-><init>([BLjava/lang/String;)V

    :try_start_0
    const-string v2, "HmacSHA1"

    invoke-static {v2}, Ljavax/crypto/Mac;->getInstance(Ljava/lang/String;)Ljavax/crypto/Mac;

    move-result-object v2

    invoke-virtual {v2, v1}, Ljavax/crypto/Mac;->init(Ljava/security/Key;)V

    invoke-virtual {v0}, Ljava/lang/String;->getBytes()[B

    move-result-object v0

    invoke-virtual {v2, v0}, Ljavax/crypto/Mac;->doFinal([B)[B

    move-result-object v0

    const/4 v1, 0x2

    invoke-static {v0, v1}, Landroid/util/Base64;->encodeToString([BI)Ljava/lang/String;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    return-object v0

    :catch_0
    move-exception v0

    const-string v1, "Something unexpected happened while making a signature for S3..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0
.end method

.method private a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;
    .locals 8
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    const-string v0, "POST"

    invoke-virtual {p1, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    new-instance v0, Lorg/apache/http/client/methods/HttpPost;

    invoke-direct {v0}, Lorg/apache/http/client/methods/HttpPost;-><init>()V

    move-object v7, v0

    :goto_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "https://"

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ".s3.amazonaws.com/"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    if-eqz p5, :cond_0

    invoke-virtual {p5}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_0

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "?"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    :cond_0
    new-instance v1, Ljava/net/URI;

    invoke-direct {v1, v0}, Ljava/net/URI;-><init>(Ljava/lang/String;)V

    invoke-virtual {v7, v1}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->setURI(Ljava/net/URI;)V

    new-instance v0, Ljava/util/Date;

    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v2

    iget-wide v4, p0, Lcom/kamcord/android/KC_Y;->u:J

    sub-long/2addr v2, v4

    invoke-direct {v0, v2, v3}, Ljava/util/Date;-><init>(J)V

    new-instance v1, Ljava/text/SimpleDateFormat;

    const-string v2, "EEE, dd MMM yyyy HH:mm:ss z"

    sget-object v3, Ljava/util/Locale;->US:Ljava/util/Locale;

    invoke-direct {v1, v2, v3}, Ljava/text/SimpleDateFormat;-><init>(Ljava/lang/String;Ljava/util/Locale;)V

    const-string v2, "GMT"

    invoke-static {v2}, Ljava/util/TimeZone;->getTimeZone(Ljava/lang/String;)Ljava/util/TimeZone;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/text/SimpleDateFormat;->setTimeZone(Ljava/util/TimeZone;)V

    invoke-virtual {v1, v0}, Ljava/text/SimpleDateFormat;->format(Ljava/util/Date;)Ljava/lang/String;

    move-result-object v1

    move-object v0, p0

    move-object v2, p1

    move-object v3, p2

    move-object v4, p3

    move-object v5, p4

    move-object v6, p5

    invoke-direct/range {v0 .. v6}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    const-string v2, "Authorization"

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "AWS "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->l:Ljava/lang/String;

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, ":"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v7, v2, v0}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "User-Agent"

    const-string v2, ""

    invoke-virtual {v7, v0, v2}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "Date"

    invoke-virtual {v7, v0, v1}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "Host"

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, p3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, ".s3.amazonaws.com"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v7, v0, v1}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "Content-Type"

    invoke-virtual {v7, v0, p2}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "x-amz-security-token"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->m:Ljava/lang/String;

    invoke-virtual {v7, v0, v1}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->addHeader(Ljava/lang/String;Ljava/lang/String;)V

    :goto_1
    return-object v7

    :cond_1
    const-string v0, "PUT"

    invoke-virtual {p1, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    new-instance v0, Lorg/apache/http/client/methods/HttpPut;

    invoke-direct {v0}, Lorg/apache/http/client/methods/HttpPut;-><init>()V

    move-object v7, v0

    goto/16 :goto_0

    :cond_2
    const/4 v7, 0x0

    goto :goto_1
.end method

.method private a(ILcom/kamcord/android/KC_Y$KC_c;)V
    .locals 12
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p2}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->g:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-lt v0, v1, :cond_1

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->i:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-ge v0, v1, :cond_0

    :cond_1
    add-int/lit8 v0, p1, 0x1

    sget-object v1, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p2}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v2

    aget v1, v1, v2

    packed-switch v1, :pswitch_data_1

    const-string v1, "PUT"

    const-string v2, "application/x-www-form-urlencoded; charset=utf-8"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "partNumber="

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v5, "&uploadId="

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    :goto_1
    sget-object v1, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p2}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v2

    aget v1, v1, v2

    packed-switch v1, :pswitch_data_2

    new-instance v1, Ljava/io/File;

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "video_directory_path"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "/video.mp4"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    :goto_2
    :try_start_0
    new-instance v2, Ljava/io/FileInputStream;

    invoke-direct {v2, v1}, Ljava/io/FileInputStream;-><init>(Ljava/io/File;)V

    iput-object v2, p0, Lcom/kamcord/android/KC_Y;->h:Ljava/io/FileInputStream;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->h:Ljava/io/FileInputStream;

    invoke-virtual {v1}, Ljava/io/FileInputStream;->getChannel()Ljava/nio/channels/FileChannel;

    move-result-object v1

    invoke-virtual {v1}, Ljava/nio/channels/FileChannel;->size()J

    move-result-wide v2

    int-to-long v4, p1

    const-wide/32 v6, 0x500000

    mul-long/2addr v6, v4

    sub-long/2addr v2, v6

    const-wide/32 v4, 0x500000

    cmp-long v1, v2, v4

    if-lez v1, :cond_2

    const-wide/32 v2, 0x500000

    :cond_2
    const/4 v4, 0x0

    const/4 v1, 0x0

    :goto_3
    int-to-long v8, v4

    cmp-long v5, v8, v6

    if-gez v5, :cond_3

    const/16 v5, 0x64

    if-ge v1, v5, :cond_3

    int-to-long v8, v4

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->h:Ljava/io/FileInputStream;

    int-to-long v10, v4

    sub-long v10, v6, v10

    invoke-virtual {v5, v10, v11}, Ljava/io/FileInputStream;->skip(J)J
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-wide v4

    add-long/2addr v4, v8

    long-to-int v4, v4

    add-int/lit8 v1, v1, 0x1

    goto :goto_3

    :pswitch_1
    const-string v1, "PUT"

    const-string v2, "application/x-www-form-urlencoded; charset=utf-8"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->s:Ljava/lang/String;

    new-instance v4, Ljava/lang/StringBuilder;

    invoke-direct {v4}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v5, "-voice"

    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "partNumber="

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v5, "&uploadId="

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    goto/16 :goto_1

    :pswitch_2
    new-instance v1, Ljava/io/File;

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "video_directory_path"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "/voice.mp4"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    goto/16 :goto_2

    :cond_3
    :try_start_1
    new-instance v1, Lcom/kamcord/android/KC_Y$KC_a;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->h:Ljava/io/FileInputStream;

    invoke-direct {v1, p0, v4, v2, v3}, Lcom/kamcord/android/KC_Y$KC_a;-><init>(Lcom/kamcord/android/KC_Y;Ljava/io/FileInputStream;J)V

    invoke-virtual {v0, v1}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->setEntity(Lorg/apache/http/HttpEntity;)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0

    new-instance v1, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v1}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    invoke-interface {v1, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v1

    invoke-interface {v1}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v0

    const/16 v2, 0xc8

    if-eq v0, v2, :cond_4

    const-string v0, "Invalid status code returned while attempting to upload file part..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0

    :catch_0
    move-exception v0

    const-string v1, "Something unexpected happened while setting the entity for uploading a part of the file to Amazon..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_4
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->h:Ljava/io/FileInputStream;

    invoke-virtual {v0}, Ljava/io/FileInputStream;->close()V

    const/4 v0, 0x0

    const-string v2, "etag"

    invoke-interface {v1, v2}, Lorg/apache/http/HttpResponse;->getHeaders(Ljava/lang/String;)[Lorg/apache/http/Header;

    move-result-object v1

    array-length v2, v1

    if-lez v2, :cond_5

    const/4 v0, 0x0

    aget-object v0, v1, v0

    invoke-interface {v0}, Lorg/apache/http/Header;->getValue()Ljava/lang/String;

    move-result-object v0

    :cond_5
    if-eqz v0, :cond_6

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->w:[Ljava/lang/String;

    aput-object v0, v1, p1

    goto/16 :goto_0

    :cond_6
    const-string v0, "Something unexpected happened when handling Amazon\'s response while uploading part of a file..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
    .end packed-switch

    :pswitch_data_1
    .packed-switch 0x1
        :pswitch_1
    .end packed-switch

    :pswitch_data_2
    .packed-switch 0x1
        :pswitch_2
    .end packed-switch
.end method

.method public static a(Lcom/kamcord/android/KC_X;)V
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, p0}, Ljava/util/Set;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method private a(Lcom/kamcord/android/KC_Y$KC_c;)V
    .locals 10
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    const-wide/32 v8, 0x500000

    const/4 v7, 0x1

    const/4 v6, 0x0

    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->f:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-lt v0, v1, :cond_1

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->h:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-ge v0, v1, :cond_0

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v0

    :goto_1
    invoke-interface {v0}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-interface {v0}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    goto :goto_1

    :cond_1
    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->e()V

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->q:Ljava/lang/String;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;)V

    :cond_2
    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_1

    const-string v1, "POST"

    const-string v2, "video/mp4"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    const-string v5, "uploads"

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    :goto_2
    new-instance v1, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v1}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    :try_start_0
    invoke-interface {v1, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v1

    invoke-interface {v1}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v1

    const/16 v2, 0xc8

    if-eq v1, v2, :cond_3

    const-string v1, "Invalid status code returned while attempting to start upload to S3..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v0

    invoke-static {v0}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :catch_0
    move-exception v0

    const-string v1, "Exception while executing request to start upload!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    throw v0

    :pswitch_1
    const-string v1, "POST"

    const-string v2, "audio/mp4a-latm"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->s:Ljava/lang/String;

    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-virtual {v0, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v4, "-voice"

    invoke-virtual {v0, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    const-string v5, "uploads"

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    goto :goto_2

    :cond_3
    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v0

    invoke-static {v0}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;

    move-result-object v0

    const-string v1, ""

    :try_start_1
    sget-object v2, Lcom/kamcord/android/KC_Y;->j:Lorg/xmlpull/v1/XmlPullParserFactory;

    invoke-virtual {v2}, Lorg/xmlpull/v1/XmlPullParserFactory;->newPullParser()Lorg/xmlpull/v1/XmlPullParser;

    move-result-object v3

    new-instance v2, Ljava/io/StringReader;

    invoke-direct {v2, v0}, Ljava/io/StringReader;-><init>(Ljava/lang/String;)V

    invoke-interface {v3, v2}, Lorg/xmlpull/v1/XmlPullParser;->setInput(Ljava/io/Reader;)V

    invoke-interface {v3}, Lorg/xmlpull/v1/XmlPullParser;->getEventType()I

    move-result v0

    move-object v2, v1

    move v1, v6

    :goto_3
    if-eq v0, v7, :cond_5

    packed-switch v0, :pswitch_data_2

    :cond_4
    :goto_4
    :pswitch_2
    invoke-interface {v3}, Lorg/xmlpull/v1/XmlPullParser;->next()I

    move-result v0

    goto :goto_3

    :pswitch_3
    invoke-interface {v3}, Lorg/xmlpull/v1/XmlPullParser;->getName()Ljava/lang/String;

    move-result-object v0

    const-string v4, "UploadId"

    invoke-virtual {v0, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_4

    move v1, v7

    goto :goto_4

    :pswitch_4
    move v1, v6

    goto :goto_4

    :pswitch_5
    if-eqz v1, :cond_4

    invoke-interface {v3}, Lorg/xmlpull/v1/XmlPullParser;->getText()Ljava/lang/String;

    move-result-object v2

    goto :goto_4

    :cond_5
    if-eqz v2, :cond_7

    const-string v0, ""

    invoke-virtual {v2, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_7

    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_3

    new-instance v0, Ljava/io/File;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "video_directory_path"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v3, "/video.mp4"

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    :goto_5
    invoke-virtual {v0}, Ljava/io/File;->length()J

    move-result-wide v0

    const-wide/32 v4, 0x500000

    div-long v4, v0, v4

    long-to-int v3, v4

    iput v3, p0, Lcom/kamcord/android/KC_Y;->f:I

    iget v3, p0, Lcom/kamcord/android/KC_Y;->f:I

    int-to-long v4, v3

    mul-long/2addr v4, v8

    cmp-long v0, v0, v4

    if-lez v0, :cond_6

    iget v0, p0, Lcom/kamcord/android/KC_Y;->f:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KC_Y;->f:I

    :cond_6
    iget v0, p0, Lcom/kamcord/android/KC_Y;->f:I

    new-array v0, v0, [Ljava/lang/String;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->w:[Ljava/lang/String;

    iput-object v2, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    :cond_7
    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_4

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->f:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_1

    goto/16 :goto_0

    :catch_1
    move-exception v0

    const-string v1, "Something unexpected happened while parsing Amazon\'s response from starting a video upload..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :pswitch_6
    :try_start_2
    new-instance v0, Ljava/io/File;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "video_directory_path"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v3, "/voice.mp4"

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    goto :goto_5

    :pswitch_7
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->h:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_1

    goto/16 :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
    .end packed-switch

    :pswitch_data_1
    .packed-switch 0x1
        :pswitch_1
    .end packed-switch

    :pswitch_data_2
    .packed-switch 0x0
        :pswitch_2
        :pswitch_2
        :pswitch_3
        :pswitch_4
        :pswitch_5
    .end packed-switch

    :pswitch_data_3
    .packed-switch 0x1
        :pswitch_6
    .end packed-switch

    :pswitch_data_4
    .packed-switch 0x1
        :pswitch_7
    .end packed-switch
.end method

.method private a(Ljava/lang/String;)V
    .locals 4

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/KC_Y;->z:J

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iget v3, p0, Lcom/kamcord/android/KC_Y;->v:I

    invoke-interface {v0, v2, v3}, Lcom/kamcord/android/KC_X;->a(Ljava/lang/String;I)V

    goto :goto_0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-static {v0, p1}, Lcom/kamcord/android/Kamcord;->notifyVideoWillBeginUploading(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method private static a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/HashSet",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ")V"
        }
    .end annotation

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    invoke-interface {v0, p0, p1, p2, p3}, Lcom/kamcord/android/KC_X;->a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method private a(Z)V
    .locals 3

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    invoke-interface {v0, v2, p1}, Lcom/kamcord/android/KC_X;->a_(Ljava/lang/String;Z)V

    goto :goto_0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-static {v0, p1}, Lcom/kamcord/android/Kamcord;->notifyVideoFinishedUploading(Ljava/lang/String;Z)V

    return-void
.end method

.method private b()V
    .locals 24

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v2

    sget-object v3, Lcom/kamcord/android/KC_Y$KC_d;->d:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v3}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v3

    if-lt v2, v3, :cond_0

    :goto_0
    return-void

    :cond_0
    :try_start_0
    const-string v2, "starting to convert wav to mp4..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v14, Landroid/media/MediaExtractor;

    invoke-direct {v14}, Landroid/media/MediaExtractor;-><init>()V

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    move-object/from16 v0, p0

    iget-object v3, v0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "video_directory_path"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "/voice.wav"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v14, v2}, Landroid/media/MediaExtractor;->setDataSource(Ljava/lang/String;)V

    const/4 v2, 0x0

    invoke-virtual {v14, v2}, Landroid/media/MediaExtractor;->selectTrack(I)V

    const/4 v2, 0x0

    invoke-virtual {v14, v2}, Landroid/media/MediaExtractor;->getTrackFormat(I)Landroid/media/MediaFormat;

    move-result-object v2

    const-string v3, "sample-rate"

    invoke-virtual {v2, v3}, Landroid/media/MediaFormat;->getInteger(Ljava/lang/String;)I

    move-result v15

    const v2, 0xa000

    invoke-static {v2}, Ljava/nio/ByteBuffer;->allocate(I)Ljava/nio/ByteBuffer;

    move-result-object v16

    const/4 v2, 0x0

    invoke-virtual {v14, v2}, Landroid/media/MediaExtractor;->getTrackFormat(I)Landroid/media/MediaFormat;

    move-result-object v2

    const-string v3, "audio/mp4a-latm"

    const-string v4, "sample-rate"

    invoke-virtual {v2, v4}, Landroid/media/MediaFormat;->getInteger(Ljava/lang/String;)I

    move-result v4

    const-string v5, "channel-count"

    invoke-virtual {v2, v5}, Landroid/media/MediaFormat;->getInteger(Ljava/lang/String;)I

    move-result v2

    invoke-static {v3, v4, v2}, Landroid/media/MediaFormat;->createAudioFormat(Ljava/lang/String;II)Landroid/media/MediaFormat;

    move-result-object v17

    const-string v2, "aac-profile"

    const/4 v3, 0x2

    move-object/from16 v0, v17

    invoke-virtual {v0, v2, v3}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    const-string v2, "bitrate"

    const v3, 0xfa00

    move-object/from16 v0, v17

    invoke-virtual {v0, v2, v3}, Landroid/media/MediaFormat;->setInteger(Ljava/lang/String;I)V

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/a/KC_a;->e()Z

    move-result v2

    if-eqz v2, :cond_2

    sget-object v2, Lcom/kamcord/android/KC_Y;->a:Ljava/lang/String;

    if-eqz v2, :cond_2

    sget-object v2, Lcom/kamcord/android/KC_Y;->a:Ljava/lang/String;

    invoke-static {v2}, Landroid/media/MediaCodec;->createByCodecName(Ljava/lang/String;)Landroid/media/MediaCodec;

    move-result-object v2

    :goto_1
    const/4 v3, 0x0

    const/4 v4, 0x0

    const/4 v5, 0x1

    move-object/from16 v0, v17

    invoke-virtual {v2, v0, v3, v4, v5}, Landroid/media/MediaCodec;->configure(Landroid/media/MediaFormat;Landroid/view/Surface;Landroid/media/MediaCrypto;I)V

    invoke-virtual {v2}, Landroid/media/MediaCodec;->start()V

    invoke-virtual {v2}, Landroid/media/MediaCodec;->getInputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v18

    invoke-virtual {v2}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v9

    const/4 v11, 0x0

    new-instance v19, Lcom/kamcord/android/core/KamcordMediaMuxer;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    move-object/from16 v0, p0

    iget-object v4, v0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v5, "video_directory_path"

    invoke-virtual {v4, v5}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "/voice.mp4"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    const/4 v4, 0x0

    move-object/from16 v0, v19

    invoke-direct {v0, v3, v4}, Lcom/kamcord/android/core/KamcordMediaMuxer;-><init>(Ljava/lang/String;I)V

    const/4 v10, -0x1

    new-instance v20, Landroid/media/MediaCodec$BufferInfo;

    invoke-direct/range {v20 .. v20}, Landroid/media/MediaCodec$BufferInfo;-><init>()V

    const-wide/16 v6, 0x0

    :goto_2
    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->clear()Ljava/nio/Buffer;

    const/4 v3, 0x0

    move-object/from16 v0, v16

    invoke-virtual {v14, v0, v3}, Landroid/media/MediaExtractor;->readSampleData(Ljava/nio/ByteBuffer;I)I

    move-result v21

    const/4 v3, -0x1

    move/from16 v0, v21

    if-eq v0, v3, :cond_7

    move-wide v12, v6

    :goto_3
    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->position()I

    move-result v3

    move/from16 v0, v21

    if-ge v3, v0, :cond_6

    const-wide/16 v4, 0x32

    invoke-virtual {v2, v4, v5}, Landroid/media/MediaCodec;->dequeueInputBuffer(J)I

    move-result v3

    if-ltz v3, :cond_1

    aget-object v4, v18, v3

    aget-object v5, v18, v3

    invoke-virtual {v5}, Ljava/nio/ByteBuffer;->clear()Ljava/nio/Buffer;

    invoke-virtual {v4}, Ljava/nio/ByteBuffer;->capacity()I

    move-result v5

    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->remaining()I

    move-result v6

    invoke-static {v5, v6}, Ljava/lang/Math;->min(II)I

    move-result v5

    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->array()[B

    move-result-object v6

    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->position()I

    move-result v7

    invoke-virtual {v4, v6, v7, v5}, Ljava/nio/ByteBuffer;->put([BII)Ljava/nio/ByteBuffer;

    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->position()I

    move-result v4

    const-wide v6, 0x412e848000000000L    # 1000000.0

    invoke-virtual/range {v16 .. v16}, Ljava/nio/ByteBuffer;->position()I

    move-result v8

    div-int/lit8 v8, v8, 0x2

    int-to-double v0, v8

    move-wide/from16 v22, v0

    mul-double v6, v6, v22

    int-to-double v0, v15

    move-wide/from16 v22, v0

    div-double v6, v6, v22

    double-to-long v6, v6

    add-int/2addr v4, v5

    move-object/from16 v0, v16

    invoke-virtual {v0, v4}, Ljava/nio/ByteBuffer;->position(I)Ljava/nio/Buffer;

    const/4 v4, 0x0

    invoke-virtual {v14}, Landroid/media/MediaExtractor;->getSampleTime()J

    move-result-wide v22

    add-long v6, v6, v22

    invoke-virtual {v14}, Landroid/media/MediaExtractor;->getSampleFlags()I

    move-result v8

    invoke-virtual/range {v2 .. v8}, Landroid/media/MediaCodec;->queueInputBuffer(IIIJI)V

    :cond_1
    const-wide/16 v4, 0x3e8

    move-object/from16 v0, v20

    invoke-virtual {v2, v0, v4, v5}, Landroid/media/MediaCodec;->dequeueOutputBuffer(Landroid/media/MediaCodec$BufferInfo;J)I

    move-result v8

    move-object/from16 v0, v20

    iget-wide v4, v0, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    cmp-long v3, v4, v12

    if-lez v3, :cond_3

    move-object/from16 v0, v20

    iget-wide v4, v0, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    move-wide v6, v4

    :goto_4
    if-ltz v8, :cond_5

    if-nez v11, :cond_9

    const/4 v4, 0x1

    const/4 v3, 0x2

    new-array v3, v3, [B

    fill-array-data v3, :array_0

    invoke-static {v3}, Ljava/nio/ByteBuffer;->wrap([B)Ljava/nio/ByteBuffer;

    move-result-object v3

    const-string v5, "csd-0"

    move-object/from16 v0, v17

    invoke-virtual {v0, v5, v3}, Landroid/media/MediaFormat;->setByteBuffer(Ljava/lang/String;Ljava/nio/ByteBuffer;)V

    move-object/from16 v0, v19

    move-object/from16 v1, v17

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->a(Landroid/media/MediaFormat;)I

    move-result v3

    invoke-virtual/range {v19 .. v19}, Lcom/kamcord/android/core/KamcordMediaMuxer;->a()V

    :goto_5
    aget-object v5, v9, v8

    if-ltz v3, :cond_4

    move-object/from16 v0, v19

    move-object/from16 v1, v20

    invoke-virtual {v0, v3, v5, v1}, Lcom/kamcord/android/core/KamcordMediaMuxer;->a(ILjava/nio/ByteBuffer;Landroid/media/MediaCodec$BufferInfo;)V

    :goto_6
    const/4 v5, 0x0

    invoke-virtual {v2, v8, v5}, Landroid/media/MediaCodec;->releaseOutputBuffer(IZ)V

    move-wide v12, v6

    move v10, v3

    move v11, v4

    goto/16 :goto_3

    :cond_2
    const-string v2, "audio/mp4a-latm"

    invoke-static {v2}, Landroid/media/MediaCodec;->createEncoderByType(Ljava/lang/String;)Landroid/media/MediaCodec;

    move-result-object v2

    goto/16 :goto_1

    :cond_3
    move-object/from16 v0, v20

    iput-wide v12, v0, Landroid/media/MediaCodec$BufferInfo;->presentationTimeUs:J

    move-wide v6, v12

    goto :goto_4

    :cond_4
    const-string v5, "Warning: samples received from codec before muxer started! Skipping samples..."

    invoke-static {v5}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_6

    :catch_0
    move-exception v2

    const-string v3, "Couldn\'t convert voice track!"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v2}, Ljava/lang/Throwable;->printStackTrace()V

    move-object/from16 v0, p0

    iget-object v2, v0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v3, "has_voice_enabled"

    const/4 v4, 0x0

    invoke-virtual {v2, v3, v4}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Z)Landroid/content/Intent;

    goto/16 :goto_0

    :cond_5
    const/4 v3, -0x3

    if-ne v8, v3, :cond_8

    :try_start_1
    invoke-virtual {v2}, Landroid/media/MediaCodec;->getOutputBuffers()[Ljava/nio/ByteBuffer;

    move-result-object v3

    :goto_7
    move-wide v12, v6

    move-object v9, v3

    goto/16 :goto_3

    :cond_6
    invoke-virtual {v14}, Landroid/media/MediaExtractor;->advance()Z

    move-wide v6, v12

    goto/16 :goto_2

    :cond_7
    invoke-virtual {v14}, Landroid/media/MediaExtractor;->release()V

    invoke-virtual {v2}, Landroid/media/MediaCodec;->stop()V

    invoke-virtual {v2}, Landroid/media/MediaCodec;->release()V

    invoke-virtual/range {v19 .. v19}, Lcom/kamcord/android/core/KamcordMediaMuxer;->b()V

    invoke-virtual/range {v19 .. v19}, Lcom/kamcord/android/core/KamcordMediaMuxer;->c()V

    const-string v2, "done converting wav to mp4."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    move-object/from16 v0, p0

    iget-wide v2, v0, Lcom/kamcord/android/KC_Y;->i:J

    new-instance v4, Ljava/io/File;

    new-instance v5, Ljava/lang/StringBuilder;

    invoke-direct {v5}, Ljava/lang/StringBuilder;-><init>()V

    move-object/from16 v0, p0

    iget-object v6, v0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v7, "video_directory_path"

    invoke-virtual {v6, v7}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v5, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    const-string v6, "/voice.mp4"

    invoke-virtual {v5, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-direct {v4, v5}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4}, Ljava/io/File;->length()J

    move-result-wide v4

    add-long/2addr v2, v4

    move-object/from16 v0, p0

    iput-wide v2, v0, Lcom/kamcord/android/KC_Y;->i:J

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_d;->d:Lcom/kamcord/android/KC_Y$KC_d;

    move-object/from16 v0, p0

    iput-object v2, v0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_0

    goto/16 :goto_0

    :cond_8
    move-object v3, v9

    goto :goto_7

    :cond_9
    move v3, v10

    move v4, v11

    goto/16 :goto_5

    :array_0
    .array-data 1
        0x12t
        0x8t
    .end array-data
.end method

.method private b(Lcom/kamcord/android/KC_Y$KC_c;)V
    .locals 8
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    const/4 v7, 0x3

    const/4 v6, 0x0

    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->g:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-lt v0, v1, :cond_1

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->i:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-ge v0, v1, :cond_0

    :cond_1
    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_1

    const-string v1, "POST"

    const-string v2, "text/xml"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v5, "uploadId="

    invoke-direct {v0, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    :goto_1
    const-string v1, "<CompleteMultipartUpload>\n"

    move-object v2, v1

    move v1, v6

    :goto_2
    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->w:[Ljava/lang/String;

    array-length v3, v3

    if-ge v1, v3, :cond_2

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "<Part><PartNumber>"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    add-int/lit8 v3, v1, 0x1

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "</PartNumber>\n<ETag>"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->w:[Ljava/lang/String;

    aget-object v3, v3, v1

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "</ETag></Part>\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    add-int/lit8 v1, v1, 0x1

    goto :goto_2

    :pswitch_1
    const-string v1, "POST"

    const-string v2, "text/xml"

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->s:Ljava/lang/String;

    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-virtual {v0, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v4, "-voice"

    invoke-virtual {v0, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v4

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v5, "uploadId="

    invoke-direct {v0, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->x:Ljava/lang/String;

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    move-object v0, p0

    invoke-direct/range {v0 .. v5}, Lcom/kamcord/android/KC_Y;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;

    move-result-object v0

    goto :goto_1

    :cond_2
    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "</CompleteMultipartUpload>"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    new-instance v2, Lorg/apache/http/entity/StringEntity;

    const-string v3, "utf-8"

    invoke-direct {v2, v1, v3}, Lorg/apache/http/entity/StringEntity;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v0, v2}, Lorg/apache/http/client/methods/HttpEntityEnclosingRequestBase;->setEntity(Lorg/apache/http/HttpEntity;)V

    new-instance v1, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v1}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    invoke-interface {v1, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v0

    const/16 v1, 0xc8

    if-eq v0, v1, :cond_3

    const-string v0, "Invalid status code returned while attempting to finish file upload..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0

    :cond_3
    sget-object v0, Lcom/kamcord/android/KC_Y$1;->a:[I

    invoke-virtual {p1}, Lcom/kamcord/android/KC_Y$KC_c;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_2

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->g()V

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "has_voice_enabled"

    invoke-virtual {v0, v1, v6}, Landroid/content/Intent;->getBooleanExtra(Ljava/lang/String;Z)Z

    move-result v0

    if-nez v0, :cond_4

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->f()V

    invoke-static {v7}, Lcom/kamcord/android/KC_m;->a(I)V

    :cond_4
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->g:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    goto/16 :goto_0

    :pswitch_2
    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->h()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->f()V

    invoke-static {v7}, Lcom/kamcord/android/KC_m;->a(I)V

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->i:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    goto/16 :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
    .end packed-switch

    :pswitch_data_1
    .packed-switch 0x1
        :pswitch_1
    .end packed-switch

    :pswitch_data_2
    .packed-switch 0x1
        :pswitch_2
    .end packed-switch
.end method

.method public static b(Lcom/kamcord/android/KC_X;)Z
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, p0}, Ljava/util/Set;->remove(Ljava/lang/Object;)Z

    move-result v0

    return v0
.end method

.method private c()V
    .locals 7
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    const/4 v6, 0x1

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_d;->j:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v1

    if-lt v0, v1, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v2, Lorg/json/JSONObject;

    invoke-direct {v2}, Lorg/json/JSONObject;-><init>()V

    :try_start_0
    const-string v0, "video_bucket_name"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "thumbnail_bucket_name"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->r:Ljava/lang/String;

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "video_id"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "has_voice_enabled"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v3, "has_voice_enabled"

    const/4 v4, 0x0

    invoke-virtual {v1, v3, v4}, Landroid/content/Intent;->getBooleanExtra(Ljava/lang/String;Z)Z

    move-result v1

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Z)Lorg/json/JSONObject;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "message"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    if-eqz v0, :cond_1

    invoke-virtual {v0}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_1

    const-string v1, "youtube_title"

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "["

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getApplicationName()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "] "

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v1, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    :goto_1
    const-string v0, "youtube_description"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v3, "description"

    invoke-virtual {v1, v3}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "app_name"

    const/4 v1, 0x0

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "youtube_keywords"

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v3, "keywords"

    invoke-virtual {v1, v3}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "is_reshare"

    iget-boolean v1, p0, Lcom/kamcord/android/KC_Y;->y:Z

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Z)Lorg/json/JSONObject;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v1, "shared_on"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getSerializableExtra(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Ljava/util/HashSet;

    new-instance v3, Lorg/json/JSONObject;

    invoke-direct {v3}, Lorg/json/JSONObject;-><init>()V

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v4

    :goto_2
    invoke-interface {v4}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-interface {v4}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    sget-object v5, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v1, v5}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v1

    const/4 v5, 0x1

    invoke-virtual {v3, v1, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Z)Lorg/json/JSONObject;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_2

    :catch_0
    move-exception v0

    const-string v1, "Something unexpected happened while constructing the parameters for informing Kamcord we finished uploading a video..."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_1
    :try_start_1
    const-string v0, "youtube_title"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "["

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getApplicationName()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v3, "] "

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "title"

    invoke-virtual {v3, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    goto/16 :goto_1

    :cond_2
    const-string v1, "shared_on"

    invoke-virtual {v2, v1, v3}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v1, "youtube"

    invoke-virtual {v0, v1}, Ljava/util/HashSet;->contains(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_3

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/b/KC_g;->a(Z)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v1

    new-instance v3, Lcom/kamcord/a/a/d/KC_j;

    const-string v4, "youtube_refresh_token"

    const-string v5, ""

    invoke-interface {v1, v4, v5}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v4, ""

    invoke-direct {v3, v1, v4}, Lcom/kamcord/a/a/d/KC_j;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v3}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v1

    if-eqz v0, :cond_4

    if-eqz v1, :cond_4

    const-string v3, "youtube_access_token"

    invoke-virtual {v2, v3, v0}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v0, "youtube_refresh_token"

    invoke-virtual {v2, v0, v1}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    const-string v1, "YouTube"

    const/4 v3, 0x1

    invoke-static {v0, v1, v3}, Lcom/kamcord/android/Kamcord;->notifyVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0

    :cond_3
    :goto_3
    :try_start_2
    const-string v0, "uploadCompletedVideo"

    invoke-static {v0, v2}, Lcom/kamcord/android/KC_d;->a(Ljava/lang/String;Lorg/json/JSONObject;)Lorg/apache/http/client/methods/HttpPost;
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_1

    move-result-object v0

    new-instance v1, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v1}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    invoke-interface {v1, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v0

    const/16 v1, 0xc8

    if-eq v0, v1, :cond_5

    const-string v0, "Invalid status code returned while attempting to complete upload."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0

    :cond_4
    :try_start_3
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    const-string v1, "YouTube"

    const/4 v3, 0x0

    invoke-static {v0, v1, v3}, Lcom/kamcord/android/Kamcord;->notifyVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V
    :try_end_3
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_0

    goto :goto_3

    :catch_1
    move-exception v0

    const-string v1, "Error creating request for informing about video upload completion."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_5
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->j:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-direct {p0, v6}, Lcom/kamcord/android/KC_Y;->a(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->c:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/UploadService;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/UploadService;->doCleanup(Ljava/lang/String;)V

    goto/16 :goto_0
.end method

.method private d()V
    .locals 4

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-interface {v0, v2, v3}, Lcom/kamcord/android/KC_X;->a(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method private e()V
    .locals 3

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    invoke-interface {v0, v2}, Lcom/kamcord/android/KC_X;->b(Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method private f()V
    .locals 3

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    invoke-interface {v0, v2}, Lcom/kamcord/android/KC_X;->a_(Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method private g()V
    .locals 2

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v0

    :goto_0
    invoke-interface {v0}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-interface {v0}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    goto :goto_0

    :cond_0
    return-void
.end method

.method private h()V
    .locals 2

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v1, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v1}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v0

    :goto_0
    invoke-interface {v0}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-interface {v0}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    goto :goto_0

    :cond_0
    return-void
.end method


# virtual methods
.method public final a()V
    .locals 4

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iget-wide v2, p0, Lcom/kamcord/android/KC_Y;->z:J

    sub-long/2addr v0, v2

    iget-wide v2, p0, Lcom/kamcord/android/KC_Y;->A:J

    cmp-long v0, v0, v2

    if-lez v0, :cond_0

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iput-wide v0, p0, Lcom/kamcord/android/KC_Y;->z:J

    :try_start_0
    iget-wide v0, p0, Lcom/kamcord/android/KC_Y;->i:J

    const-wide/16 v2, 0x0

    cmp-long v0, v0, v2

    if-lez v0, :cond_0

    iget-wide v0, p0, Lcom/kamcord/android/KC_Y;->g:J

    long-to-float v0, v0

    iget-wide v2, p0, Lcom/kamcord/android/KC_Y;->i:J

    long-to-float v1, v2

    div-float v1, v0, v1

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    sget-object v2, Lcom/kamcord/android/KC_Y;->e:Ljava/util/Set;

    invoke-interface {v0, v2}, Ljava/util/Set;->addAll(Ljava/util/Collection;)Z

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_0
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_X;

    iget-object v3, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    invoke-interface {v0, v3, v1}, Lcom/kamcord/android/KC_X;->a(Ljava/lang/String;F)V
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    :cond_0
    :goto_1
    return-void

    :cond_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->notifyVideoUploadProgressed(Ljava/lang/String;F)V
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0

    goto :goto_1
.end method

.method public final run()V
    .locals 11

    const/16 v10, 0xc8

    const/4 v3, 0x1

    const/4 v1, 0x0

    iput v1, p0, Lcom/kamcord/android/KC_Y;->v:I

    :goto_0
    iget v0, p0, Lcom/kamcord/android/KC_Y;->v:I

    const/4 v2, 0x3

    if-ge v0, v2, :cond_10

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_d;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v2

    if-lt v0, v2, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_d;->j:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v2

    if-ne v0, v2, :cond_2

    const/4 v0, 0x1

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->a(Z)V

    move v0, v1

    :goto_1
    if-eqz v0, :cond_f

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_d;->c:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v2

    if-lt v0, v2, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->d()V

    :cond_0
    :goto_2
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v2, "has_voice_enabled"

    const/4 v4, 0x0

    invoke-virtual {v0, v2, v4}, Landroid/content/Intent;->getBooleanExtra(Ljava/lang/String;Z)Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->b()V

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v0

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_d;->e:Lcom/kamcord/android/KC_Y$KC_d;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_Y$KC_d;->ordinal()I

    move-result v2

    if-ge v0, v2, :cond_b

    new-instance v0, Lorg/apache/http/client/methods/HttpPost;

    const-string v2, "https://www.kamcord.com/api/auth/sts/"

    invoke-direct {v0, v2}, Lorg/apache/http/client/methods/HttpPost;-><init>(Ljava/lang/String;)V

    const-string v2, "Content-Type"

    const-string v4, "application/json"

    invoke-virtual {v0, v2, v4}, Lorg/apache/http/client/methods/HttpPost;->setHeader(Ljava/lang/String;Ljava/lang/String;)V

    new-instance v2, Lorg/apache/http/entity/StringEntity;

    const-string v4, "{}"

    invoke-direct {v2, v4}, Lorg/apache/http/entity/StringEntity;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v2}, Lorg/apache/http/client/methods/HttpPost;->setEntity(Lorg/apache/http/HttpEntity;)V

    new-instance v2, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v2}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    invoke-interface {v2, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v2

    invoke-interface {v2}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v2

    if-eq v2, v10, :cond_a

    const-string v0, "Invalid status code returned while attempting to get S3 credentials..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :catch_0
    move-exception v0

    const-string v2, "Something unexpected happened during video upload, trying again..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->a:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    iget v0, p0, Lcom/kamcord/android/KC_Y;->v:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KC_Y;->v:I

    goto/16 :goto_0

    :cond_2
    move v0, v3

    goto :goto_1

    :cond_3
    :try_start_1
    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_4

    const/4 v0, 0x0

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->a(Z)V

    move v0, v1

    goto/16 :goto_1

    :cond_4
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->b:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KC_Y;->f:I

    new-instance v0, Ljava/io/File;

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v5, "video_directory_path"

    invoke-virtual {v4, v5}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v4, "/video.mp4"

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v0, v2}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Ljava/io/File;->length()J

    move-result-wide v4

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->i:J

    move v0, v3

    goto/16 :goto_1

    :cond_5
    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_0

    :try_start_2
    const-string v2, "platform"

    const-string v4, "Android"

    invoke-virtual {v0, v2, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v2, "platform_version"

    sget-object v4, Landroid/os/Build$VERSION;->RELEASE:Ljava/lang/String;

    invoke-virtual {v0, v2, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v2, "platform_api_level"

    sget v4, Landroid/os/Build$VERSION;->SDK_INT:I

    invoke-virtual {v0, v2, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;I)Lorg/json/JSONObject;

    new-instance v2, Lorg/json/JSONObject;

    invoke-direct {v2}, Lorg/json/JSONObject;-><init>()V

    const-string v4, "title"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "title"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v5, "message"

    invoke-virtual {v4, v5}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_6

    const-string v4, "message"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "message"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    :cond_6
    const-string v4, "description"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "description"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v4, "facebook_description"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "facebook_description"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v4, "twitter_description"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "twitter_description"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v4, "keywords"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "keywords"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v2, v4, v5}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    const-string v4, "video_duration"

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "video_duration"

    const-wide/16 v8, 0x0

    invoke-virtual {v5, v6, v8, v9}, Landroid/content/Intent;->getDoubleExtra(Ljava/lang/String;D)D

    move-result-wide v6

    invoke-virtual {v2, v4, v6, v7}, Lorg/json/JSONObject;->put(Ljava/lang/String;D)Lorg/json/JSONObject;

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v5, "developer_metadata_obj"

    invoke-virtual {v4, v5}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_7

    new-instance v4, Lorg/json/JSONObject;

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v6, "developer_metadata_obj"

    invoke-virtual {v5, v6}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v5

    invoke-direct {v4, v5}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    const-string v5, "developer_metadata_obj"

    invoke-virtual {v2, v5, v4}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;

    :cond_7
    const-string v4, "video_params"

    invoke-virtual {v0, v4, v2}, Lorg/json/JSONObject;->put(Ljava/lang/String;Ljava/lang/Object;)Lorg/json/JSONObject;
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_1
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_0

    :try_start_3
    const-string v2, "uploadParamsVideo"

    invoke-static {v2, v0}, Lcom/kamcord/android/KC_d;->a(Ljava/lang/String;Lorg/json/JSONObject;)Lorg/apache/http/client/methods/HttpPost;

    move-result-object v0

    new-instance v2, Lorg/apache/http/impl/client/DefaultHttpClient;

    invoke-direct {v2}, Lorg/apache/http/impl/client/DefaultHttpClient;-><init>()V

    invoke-interface {v2, v0}, Lorg/apache/http/client/HttpClient;->execute(Lorg/apache/http/client/methods/HttpUriRequest;)Lorg/apache/http/HttpResponse;

    move-result-object v0

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getStatusLine()Lorg/apache/http/StatusLine;

    move-result-object v2

    invoke-interface {v2}, Lorg/apache/http/StatusLine;->getStatusCode()I

    move-result v2

    if-eq v2, v10, :cond_8

    const-string v2, "Invalid status code returned while attempting to get a video id."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v0

    invoke-static {v0}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/Exception;

    invoke-direct {v0}, Ljava/lang/Exception;-><init>()V

    throw v0

    :catch_1
    move-exception v0

    const-string v2, "Something unexpected happened while constructing the parameters for requesting a video id..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_8
    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v0

    invoke-static {v0}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;
    :try_end_3
    .catch Ljava/lang/Throwable; {:try_start_3 .. :try_end_3} :catch_0

    move-result-object v0

    :try_start_4
    new-instance v2, Lorg/json/JSONObject;

    invoke-direct {v2, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    const-string v0, "video_id"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    const-string v0, "video_bucket_name"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->p:Ljava/lang/String;

    const-string v0, "thumbnail_bucket_name"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->r:Ljava/lang/String;

    const-string v0, "thumbnail_bucket_name"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->s:Ljava/lang/String;

    const-string v0, "video_url"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->q:Ljava/lang/String;

    invoke-static {}, Ljava/lang/System;->currentTimeMillis()J

    move-result-wide v4

    const-string v0, "added_at"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getLong(Ljava/lang/String;)J

    move-result-wide v6

    const-wide/16 v8, 0x3e8

    mul-long/2addr v6, v8

    sub-long/2addr v4, v6

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->u:J

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "Setting clock skew to "

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-wide v4, p0, Lcom/kamcord/android/KC_Y;->u:J

    invoke-virtual {v0, v4, v5}, Ljava/lang/StringBuilder;->append(J)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->c:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->d()V

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v2, "shared_on"

    invoke-virtual {v0, v2}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v2, "shared_on"

    invoke-virtual {v0, v2}, Landroid/content/Intent;->getSerializableExtra(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Ljava/util/HashSet;

    const-string v2, ""

    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v5, "message"

    invoke-virtual {v4, v5}, Landroid/content/Intent;->hasExtra(Ljava/lang/String;)Z

    move-result v4

    if-eqz v4, :cond_9

    iget-object v2, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v4, "message"

    invoke-virtual {v2, v4}, Landroid/content/Intent;->getStringExtra(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    :cond_9
    iget-object v4, p0, Lcom/kamcord/android/KC_Y;->o:Ljava/lang/String;

    iget-object v5, p0, Lcom/kamcord/android/KC_Y;->n:Ljava/lang/String;

    invoke-static {v0, v4, v5, v2}, Lcom/kamcord/android/KC_Y;->a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    :try_end_4
    .catch Ljava/lang/Exception; {:try_start_4 .. :try_end_4} :catch_2
    .catch Ljava/lang/Throwable; {:try_start_4 .. :try_end_4} :catch_0

    goto/16 :goto_2

    :catch_2
    move-exception v0

    :try_start_5
    const-string v2, "Something unexpected happened while parsing server\'s response from requesting a video id..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_a
    invoke-interface {v0}, Lorg/apache/http/HttpResponse;->getEntity()Lorg/apache/http/HttpEntity;

    move-result-object v0

    invoke-static {v0}, Lorg/apache/http/util/EntityUtils;->toString(Lorg/apache/http/HttpEntity;)Ljava/lang/String;
    :try_end_5
    .catch Ljava/lang/Throwable; {:try_start_5 .. :try_end_5} :catch_0

    move-result-object v0

    :try_start_6
    new-instance v2, Lorg/json/JSONObject;

    invoke-direct {v2, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    const-string v0, "secret_key"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->k:Ljava/lang/String;

    const-string v0, "access_key"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->l:Ljava/lang/String;

    const-string v0, "session_token"

    invoke-virtual {v2, v0}, Lorg/json/JSONObject;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->m:Ljava/lang/String;

    const-wide/16 v4, 0x0

    iput-wide v4, p0, Lcom/kamcord/android/KC_Y;->g:J

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_d;->e:Lcom/kamcord/android/KC_Y$KC_d;

    iput-object v0, p0, Lcom/kamcord/android/KC_Y;->b:Lcom/kamcord/android/KC_Y$KC_d;
    :try_end_6
    .catch Ljava/lang/Exception; {:try_start_6 .. :try_end_6} :catch_3
    .catch Ljava/lang/Throwable; {:try_start_6 .. :try_end_6} :catch_0

    :cond_b
    :try_start_7
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_c;->a:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->a(Lcom/kamcord/android/KC_Y$KC_c;)V

    move v0, v1

    :goto_3
    iget v2, p0, Lcom/kamcord/android/KC_Y;->f:I

    if-ge v0, v2, :cond_c

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_c;->a:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0, v2}, Lcom/kamcord/android/KC_Y;->a(ILcom/kamcord/android/KC_Y$KC_c;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_3

    :catch_3
    move-exception v0

    const-string v2, "Something unexpected happened while parsing S3 credentials..."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    throw v0

    :cond_c
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_c;->a:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->b(Lcom/kamcord/android/KC_Y$KC_c;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_Y;->d:Landroid/content/Intent;

    const-string v2, "has_voice_enabled"

    const/4 v4, 0x0

    invoke-virtual {v0, v2, v4}, Landroid/content/Intent;->getBooleanExtra(Ljava/lang/String;Z)Z

    move-result v0

    if-eqz v0, :cond_e

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_c;->b:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->a(Lcom/kamcord/android/KC_Y$KC_c;)V

    move v0, v1

    :goto_4
    iget v2, p0, Lcom/kamcord/android/KC_Y;->f:I

    if-ge v0, v2, :cond_d

    sget-object v2, Lcom/kamcord/android/KC_Y$KC_c;->b:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0, v2}, Lcom/kamcord/android/KC_Y;->a(ILcom/kamcord/android/KC_Y$KC_c;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_4

    :cond_d
    sget-object v0, Lcom/kamcord/android/KC_Y$KC_c;->b:Lcom/kamcord/android/KC_Y$KC_c;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_Y;->b(Lcom/kamcord/android/KC_Y$KC_c;)V

    :cond_e
    invoke-direct {p0}, Lcom/kamcord/android/KC_Y;->c()V
    :try_end_7
    .catch Ljava/lang/Throwable; {:try_start_7 .. :try_end_7} :catch_0

    :cond_f
    :goto_5
    return-void

    :cond_10
    const-string v0, "Unable to upload video, giving up."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-direct {p0, v1}, Lcom/kamcord/android/KC_Y;->a(Z)V

    goto :goto_5
.end method
