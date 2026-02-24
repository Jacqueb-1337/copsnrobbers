.class Lcom/kamcord/android/KC_z;
.super Landroid/webkit/WebViewClient;


# instance fields
.field private a:Ljava/lang/String;

.field private b:Lcom/kamcord/a/a/e/KC_c;

.field private c:Landroid/app/Activity;


# direct methods
.method constructor <init>(Ljava/lang/String;Lcom/kamcord/a/a/e/KC_c;Landroid/app/Activity;)V
    .locals 0

    invoke-direct {p0}, Landroid/webkit/WebViewClient;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_z;->a:Ljava/lang/String;

    iput-object p2, p0, Lcom/kamcord/android/KC_z;->b:Lcom/kamcord/a/a/e/KC_c;

    iput-object p3, p0, Lcom/kamcord/android/KC_z;->c:Landroid/app/Activity;

    return-void
.end method


# virtual methods
.method public shouldOverrideUrlLoading(Landroid/webkit/WebView;Ljava/lang/String;)Z
    .locals 6

    const/4 v0, 0x1

    const/4 v1, 0x0

    if-eqz p2, :cond_1

    iget-object v2, p0, Lcom/kamcord/android/KC_z;->a:Ljava/lang/String;

    invoke-virtual {p2, v2}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v2

    if-eqz v2, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    iget-object v2, p0, Lcom/kamcord/android/KC_z;->b:Lcom/kamcord/a/a/e/KC_c;

    const-string v3, "code=([^&]+)"

    invoke-static {v3}, Ljava/util/regex/Pattern;->compile(Ljava/lang/String;)Ljava/util/regex/Pattern;

    move-result-object v3

    invoke-virtual {v3, p2}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v3

    invoke-virtual {v3}, Ljava/util/regex/Matcher;->find()Z

    move-result v4

    if-eqz v4, :cond_0

    invoke-virtual {v3, v0}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v3

    invoke-static {v3}, Lcom/kamcord/a/a/g/KC_a;->b(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    new-instance v4, Lcom/kamcord/a/a/d/KC_l;

    invoke-direct {v4, v3}, Lcom/kamcord/a/a/d/KC_l;-><init>(Ljava/lang/String;)V

    new-instance v3, Lcom/kamcord/android/KC_A;

    const/4 v5, 0x0

    invoke-direct {v3, v2, v4, v5}, Lcom/kamcord/android/KC_A;-><init>(Lcom/kamcord/a/a/e/KC_c;Lcom/kamcord/a/a/d/KC_l;Lcom/kamcord/a/a/d/KC_j;)V

    new-array v1, v1, [Ljava/lang/Void;

    invoke-virtual {v3, v1}, Lcom/kamcord/android/KC_A;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    :goto_0
    iget-object v1, p0, Lcom/kamcord/android/KC_z;->c:Landroid/app/Activity;

    invoke-virtual {v1}, Landroid/app/Activity;->finish()V

    :goto_1
    return v0

    :cond_0
    invoke-interface {v2}, Lcom/kamcord/a/a/e/KC_c;->a()V

    goto :goto_0

    :cond_1
    move v0, v1

    goto :goto_1
.end method
