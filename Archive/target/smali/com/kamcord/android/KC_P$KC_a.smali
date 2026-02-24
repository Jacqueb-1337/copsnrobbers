.class final Lcom/kamcord/android/KC_P$KC_a;
.super Lcom/kamcord/android/KC_ab$KC_a;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_P;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_P;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_P;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_P$KC_a;->a:Lcom/kamcord/android/KC_P;

    invoke-direct {p0}, Lcom/kamcord/android/KC_ab$KC_a;-><init>()V

    return-void
.end method


# virtual methods
.method public final shouldOverrideUrlLoading(Landroid/webkit/WebView;Ljava/lang/String;)Z
    .locals 5

    const/4 v4, 0x2

    const/4 v0, 0x1

    const-string v1, "^(kc-(.*?)://)(.*)"

    invoke-static {v1}, Ljava/util/regex/Pattern;->compile(Ljava/lang/String;)Ljava/util/regex/Pattern;

    move-result-object v1

    invoke-virtual {v1, p2}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v1

    invoke-virtual {v1}, Ljava/util/regex/Matcher;->find()Z

    move-result v2

    if-eqz v2, :cond_1

    invoke-virtual {v1, v0}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/String;->length()I

    move-result v2

    if-lez v2, :cond_1

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Link of type kc-"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v4}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " clicked."

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v1, v4}, Ljava/util/regex/Matcher;->group(I)Ljava/lang/String;

    move-result-object v1

    const-string v2, "forgotpassword"

    invoke-virtual {v1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_k;

    invoke-direct {v1}, Lcom/kamcord/android/KC_k;-><init>()V

    iget-object v2, p0, Lcom/kamcord/android/KC_P$KC_a;->a:Lcom/kamcord/android/KC_P;

    invoke-static {v2}, Lcom/kamcord/android/KC_P;->d(Lcom/kamcord/android/KC_P;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v2

    invoke-virtual {v2, v1}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    :cond_0
    :goto_0
    return v0

    :cond_1
    const/4 v0, 0x0

    goto :goto_0
.end method
